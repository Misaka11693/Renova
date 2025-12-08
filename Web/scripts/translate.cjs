require('dotenv').config({ path: ".env" });
require('dotenv').config({ path: ".env.local" });
const fs = require('fs');
const path = require('path');
const API_URL = 'https://api.deepseek.com/v1/chat/completions';
const API_KEY = process.env.DEEPSEEK_API_KEY;
const SOURCE_LANG = 'zh-cn';
const LOCALE_FILE = path.resolve(__dirname, '../lang/index.json');

// 引入进度条库
const cliProgress = require('cli-progress');
const colors = require('colors');

async function translateBatch(texts, targetLang) {
	// 构建批量内容：每行以[index]开头
	const batchContent = texts.map((text, index) => `[${index}] ${text}`).join('\n');

	const response = await fetch(API_URL, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json',
			Authorization: `Bearer ${API_KEY}`,
		},
		body: JSON.stringify({
			model: 'deepseek-chat',
			messages: [
				{
					role: 'system',
					content: `作为企业软件系统专业翻译，严格遵守以下铁律：

■ 核心原则
1. 严格逐符号翻译（${SOURCE_LANG}→${targetLang}）
2. 禁止添加/删除/改写任何内容
3. 保持批量翻译的编号格式

■ 符号保留规则
! 所有符号必须原样保留：
• 编程符号：\${ } <% %> @ # & | 
• UI占位符：{0} %s [ ] 
• 货币单位：¥100.00 kg cm²
• 中文符号：【 】 《 》 ：

■ 中文符号位置规范
# 三级处理机制：
1. 成对符号必须保持完整结构：
   ✓ 正确：【Warning】Text
   ✗ 禁止：Warning【 】Text
   
2. 独立符号位置：
   • 优先句尾 → Text】?
   • 次选句首 → 】Text?
   • 禁止句中 → Text】Text?

3. 跨字符串符号处理：
   • 前段含【时 → 保留在段尾（"Synchronize【"）
   • 后段含】时 → 保留在段首（"】authorization data?"）
   • 符号后接字母时添加空格：】 Authorization

■ 语法规范
• 外文 → 被动语态（"Item was created"）
• 中文 → 主动语态（"已创建项目"）
• 禁止推测上下文（只翻译当前字符串内容）

■ 错误预防（绝对禁止）
✗ 将中文符号改为西式符号（】→]）
✗ 移动非中文符号位置
✗ 添加原文不存在的内容
✗ 合并/拆分原始字符串

■ 批量处理
▸ 严格保持原始JSON结构
▸ 语言键名精确匹配（zh-cn/en/it等）`
				},
				{
					role: 'user',
					content: batchContent,
				},
			],
			temperature: 0.3,
			max_tokens: 4000,
		}),
	});

	const data = await response.json();
	if (!response.ok || !data.choices || !data.choices[0]?.message?.content) {
		const errorMsg = data.error?.message || `HTTP ${response.status}: ${response.statusText}`;
		throw new Error(`翻译API返回错误：${errorMsg}`);
	}

	// 解析批量响应
	const batchResult = data.choices[0].message.content.trim();
	const translations = {};

	// 按行分割结果
	const lines = batchResult.split('\n');
	for (const line of lines) {
		// 使用更精确的匹配模式
		const match = line.match(/^\[(\d+)\]\s*(.+)/);
		if (match) {
			const index = parseInt(match[1]);
			translations[index] = match[2].trim();
		}
	}

	return translations;
}

function extractTargetLangs(localeData) {
	const allLangs = new Set();
	for (const translations of Object.values(localeData)) {
		for (const lang of Object.keys(translations)) {
			if (lang !== SOURCE_LANG) {
				allLangs.add(lang);
			}
		}
	}
	return [...allLangs];
}

function groupTasksByLang(localeData, targetLangs) {
	const tasks = {};

	for (const lang of targetLangs) {
		tasks[lang] = {
			keys: [],
			texts: [],
		};
	}

	for (const [key, translations] of Object.entries(localeData)) {
		const sourceText = translations[SOURCE_LANG];
		if (!sourceText) {
			console.warn(`⚠️ 缺少源语言(${SOURCE_LANG})文本: ${key}`);
			continue;
		}

		for (const lang of targetLangs) {
			if (!translations[lang] || translations[lang].trim() === '') {
				tasks[lang].keys.push(key);
				tasks[lang].texts.push(sourceText);
			}
		}
	}

	return tasks;
}

async function main() {
	// 读取语言文件
	const rawData = fs.readFileSync(LOCALE_FILE);
	const localeData = JSON.parse(rawData);
	const TARGET_LANGS = extractTargetLangs(localeData);
	const langTasks = groupTasksByLang(localeData, TARGET_LANGS);

	let totalUpdated = 0;
	const BATCH_SIZE = 10;
	// 创建多进度条容器
	const multibar = new cliProgress.MultiBar(
		{
			format: '{lang} |' + colors.cyan('{bar}') + '| {percentage}% | {value}/{total} 条',
			barCompleteChar: '\u2588',
			barIncompleteChar: '\u2591',
			hideCursor: true,
			clearOnComplete: true,
			stopOnComplete: true,
		},
		cliProgress.Presets.shades_grey
	);
	// 为每个语言创建进度条
	const progressBars = {};
	for (const lang of TARGET_LANGS) {
		if (langTasks[lang].texts.length > 0) {
			progressBars[lang] = multibar.create(langTasks[lang].texts.length, 0, {
				lang: lang.padEnd(6, ' '),
			});
		}
	}

	// 并行处理所有语言
	await Promise.all(
		Object.entries(langTasks).map(async ([lang, task]) => {
			if (task.texts.length === 0) return;

			// 分批处理
			for (let i = 0; i < task.texts.length; i += BATCH_SIZE) {
				const batchKeys = task.keys.slice(i, i + BATCH_SIZE);
				const batchTexts = task.texts.slice(i, i + BATCH_SIZE);

				try {
					const batchResults = await translateBatch(batchTexts, lang);

					// 更新翻译结果
					batchKeys.forEach((key, index) => {
						if (batchResults[index] !== undefined) {
							localeData[key][lang] = batchResults[index];
							totalUpdated++;
						} else {
							console.error(`❌ 缺失翻译结果 [${key}@${lang}]`);
							localeData[key][lang] = `[BATCH_ERROR] ${localeData[key][SOURCE_LANG]}`;
						}
					});

					// 更新进度条
					progressBars[lang].increment(batchTexts.length);

					// 每批处理后保存进度
					fs.writeFileSync(LOCALE_FILE, JSON.stringify(localeData, null, 2));

					// 添加请求间隔避免速率限制
					await new Promise((resolve) => setTimeout(resolve, 300));
				} catch (error) {
					console.error(`\n❌ 批次翻译失败 [${lang}]:`, error.message);
					// 标记失败条目
					batchKeys.forEach((key) => {
						localeData[key][lang] = `[TRANSLATION_FAILED] ${localeData[key][SOURCE_LANG]}`;
					});
					// 跳过当前批次继续处理
					progressBars[lang].increment(batchTexts.length);
				}
			}
		})
	);

	// 停止所有进度条
	multibar.stop();

	// 最终保存
	fs.writeFileSync(LOCALE_FILE, JSON.stringify(localeData, null, 2));

	// 显示最终结果
	if (totalUpdated > 0) {
		console.log(`\n✅ 翻译完成! 共更新 ${totalUpdated} 处翻译`);
	} else {
		console.log('\nℹ️ 没有需要更新的翻译');
	}
}

main().catch(console.error);
