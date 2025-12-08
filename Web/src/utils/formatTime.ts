/**
 * 时间日期转换
 * @param date 当前时间，new Date() 格式
 * @param format 需要转换的时间格式字符串
 * @description format 字符串随意，如 `YYYY-mm、YYYY-mm-dd`
 * @description format 季度："YYYY-mm-dd HH:MM:SS QQQQ"
 * @description format 星期："YYYY-mm-dd HH:MM:SS WWW"
 * @description format 几周："YYYY-mm-dd HH:MM:SS ZZZ"
 * @description format 季度 + 星期 + 几周："YYYY-mm-dd HH:MM:SS WWW QQQQ ZZZ"
 * @returns 返回拼接后的时间字符串
 */
export function formatDate(date: Date, format: string): string {
	let we = date.getDay(); // 星期
	let z = getWeek(date); // 周
	let qut = Math.floor((date.getMonth() + 3) / 3).toString(); // 季度
	const opt: { [key: string]: string } = {
		'Y+': date.getFullYear().toString(), // 年
		'm+': (date.getMonth() + 1).toString(), // 月(月份从0开始，要+1)
		'd+': date.getDate().toString(), // 日
		'H+': date.getHours().toString(), // 时
		'M+': date.getMinutes().toString(), // 分
		'S+': date.getSeconds().toString(), // 秒
		'q+': qut, // 季度
	};
	// 中文数字 (星期)
	const week: { [key: string]: string } = {
		'0': '日',
		'1': '一',
		'2': '二',
		'3': '三',
		'4': '四',
		'5': '五',
		'6': '六',
	};
	// 中文数字（季度）
	const quarter: { [key: string]: string } = {
		'1': '一',
		'2': '二',
		'3': '三',
		'4': '四',
	};
	if (/(W+)/.test(format)) format = format.replace(RegExp.$1, RegExp.$1.length > 1 ? (RegExp.$1.length > 2 ? '星期' + week[we] : '周' + week[we]) : week[we]);
	if (/(Q+)/.test(format)) format = format.replace(RegExp.$1, RegExp.$1.length == 4 ? '第' + quarter[qut] + '季度' : quarter[qut]);
	if (/(Z+)/.test(format)) format = format.replace(RegExp.$1, RegExp.$1.length == 3 ? '第' + z + '周' : z + '');
	for (let k in opt) {
		let r = new RegExp('(' + k + ')').exec(format);
		// 若输入的长度不为1，则前面补零
		if (r) format = format.replace(r[1], RegExp.$1.length == 1 ? opt[k] : opt[k].padStart(RegExp.$1.length, '0'));
	}
	return format;
}

/**
 * 获取当前日期是第几周
 * @param dateTime 当前传入的日期值
 * @returns 返回第几周数字值
 */
export function getWeek(dateTime: Date): number {
	let temptTime = new Date(dateTime.getTime());
	// 周几
	let weekday = temptTime.getDay() || 7;
	// 周1+5天=周六
	temptTime.setDate(temptTime.getDate() - weekday + 1 + 5);
	let firstDay = new Date(temptTime.getFullYear(), 0, 1);
	let dayOfWeek = firstDay.getDay();
	let spendDay = 1;
	if (dayOfWeek != 0) spendDay = 7 - dayOfWeek + 1;
	firstDay = new Date(temptTime.getFullYear(), 0, 1 + spendDay);
	let d = Math.ceil((temptTime.valueOf() - firstDay.valueOf()) / 86400000);
	let result = Math.ceil(d / 7);
	return result;
}

/**
 * 将时间转换为 `几秒前`、`几分钟前`、`几小时前`、`几天前`
 * @param param 当前时间，new Date() 格式或者字符串时间格式
 * @param format 需要转换的时间格式字符串
 * @description param 10秒：  10 * 1000
 * @description param 1分：   60 * 1000
 * @description param 1小时： 60 * 60 * 1000
 * @description param 24小时：60 * 60 * 24 * 1000
 * @description param 3天：   60 * 60* 24 * 1000 * 3
 * @returns 返回拼接后的时间字符串
 */
export function formatPast(param: string | Date, format: string = 'YYYY-mm-dd'): string {
	// 传入格式处理、存储转换值
	let t: any, s: number;
	// 获取js 时间戳
	let time: number = new Date().getTime();
	// 是否是对象
	typeof param === 'string' || 'object' ? (t = new Date(param).getTime()) : (t = param);
	// 当前时间戳 - 传入时间戳
	time = Number.parseInt(`${time - t}`);
	if (time < 10000) {
		// 10秒内
		return '刚刚';
	} else if (time < 60000 && time >= 10000) {
		// 超过10秒少于1分钟内
		s = Math.floor(time / 1000);
		return `${s}秒前`;
	} else if (time < 3600000 && time >= 60000) {
		// 超过1分钟少于1小时
		s = Math.floor(time / 60000);
		return `${s}分钟前`;
	} else if (time < 86400000 && time >= 3600000) {
		// 超过1小时少于24小时
		s = Math.floor(time / 3600000);
		return `${s}小时前`;
	} else if (time < 259200000 && time >= 86400000) {
		// 超过1天少于3天内
		s = Math.floor(time / 86400000);
		return `${s}天前`;
	} else {
		// 超过3天
		let date = typeof param === 'string' || 'object' ? new Date(param) : param;
		return formatDate(date, format);
	}
}

/**
 * 时间问候语
 * @param param 当前时间，new Date() 格式
 * @description param 调用 `formatAxis(new Date())` 输出 `上午好`
 * @returns 返回拼接后的时间字符串
 */
export function formatAxis(param: Date): string {
	let hour: number = new Date(param).getHours();
	if (hour < 6) return '凌晨好';
	else if (hour < 9) return '早上好';
	else if (hour < 12) return '上午好';
	else if (hour < 14) return '中午好';
	else if (hour < 17) return '下午好';
	else if (hour < 19) return '傍晚好';
	else if (hour < 22) return '晚上好';
	else return '夜里好';
}

/**
 * 获取两个时间相差的秒数
 * @dateBegin 开始时间，new Date() 格式
 * @dateEnd 结束时间，new Date() 格式
 * @returns 返回秒数
 */
export function getTimeDiff(dateBegin: Date, dateEnd: Date,) {
	var dateDiff = dateEnd.getTime() - dateBegin.getTime();
	return dateDiff / 1000;
}

/**
 * 格式化两个时间差
 * @dateBegin 开始时间，new Date() 格式
 * @dateEnd 结束时间，new Date() 格式
 * @description dateBegin 2025-1-1，dateEnd 2025-1-2 10:10:10 ：   1天10时10分10秒
 * @returns 返回拼接后的时间字符串
 */
export function formatTimeDiff(dateBegin: Date, dateEnd: Date,) {
	var dateDiff = dateEnd.getTime() - dateBegin.getTime();//时间差的毫秒数
	var dayDiff = Math.floor(dateDiff / (24 * 3600 * 1000));//计算出相差天数
	var leave1 = dateDiff % (24 * 3600 * 1000)    //计算天数后剩余的毫秒数
	var hours = Math.floor(leave1 / (3600 * 1000))//计算出小时数
	//计算相差分钟数
	var leave2 = leave1 % (3600 * 1000)    //计算小时数后剩余的毫秒数
	var minutes = Math.floor(leave2 / (60 * 1000))//计算相差分钟数
	//计算相差秒数
	var leave3 = leave2 % (60 * 1000)      //计算分钟数后剩余的毫秒数
	var seconds = Math.round(leave3 / 1000);
	var result = "";
	if (dayDiff > 0) {
		result += dayDiff + "天";
	}
	if (hours > 0) {
		result += hours + "时";
	}
	if (minutes > 0) {
		result += minutes + "分";
	}
	if (seconds >= 0) {
		result += seconds + "秒";
	}
	return result;
}

/**
 * 将时间字符串格式化为 `YYYY-mm-dd HH:MM:SS` 格式
 * @param timeStr 时间字符串，支持非时区格式，如 `YYYYmmdd`、`YYYYmmddHH`、`YYYYmmddHHMM`、`YYYYmmddHHMMSS`、`YYYY/mm/dd`、`YYYY年mm月dd日`等
 * @param length 返回的时间字符串长度，默认0表示自动识别长度
 * @returns 返回格式化后的时间字符串
 */
export function formatDateString(timeStr: string | null | undefined, length: number = 0): string {
	if (!timeStr) return '';

	let str = timeStr.replace(/\D/g, '');
	let len = str.length;

	if (len <= 4) return str;

	// 处理奇数长度：在最后一位前补0
	if (len & 1) {
		len++;
		str = str.slice(0, -1) + '0' + str.slice(-1);
	}

	str = str.padEnd(14, '0');

	// 提取各时间部分
	const [year, month, day, hour, minute, second] = [0, 4, 6, 8, 10, 12].map(index => str.slice(index, (index || 2) + 2));

	// 计算长度，长度为10时，显示分钟
	const targetLength = length > 0 ? length : len + (len - 4) / 2 + (len == 10 ? 3 : 0);

	// 处理0/00的修正函数
	const fixZero = (value: string) => ['0', '00'].includes(value) ? '01' : value;

	// 生成完整格式字符串
	const fullFormat = `${year}-${fixZero(month)}-${fixZero(day)} ${hour}:${minute}:${second}`;

	return fullFormat.slice(0, targetLength);
}