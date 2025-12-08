// 通用函数
import useClipboard from 'vue-clipboard3';
import { ElMessage } from 'element-plus';
import { formatDate } from '/@/utils/formatTime';

export default function () {
	const { toClipboard } = useClipboard();

	// 百分比格式化
	const percentFormat = (row: EmptyArrayType, column: number, cellValue: string) => {
		return cellValue ? `${cellValue}%` : '-';
	};
	// 列表日期时间格式化
	const dateFormatYMD = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		return formatDate(new Date(cellValue), 'YYYY-mm-dd');
	};
	// 列表日期时间格式化
	const dateFormatYMDHMS = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		return formatDate(new Date(cellValue), 'YYYY-mm-dd HH:MM:SS');
	};
	// 列表日期时间格式化
	const dateFormatHMS = (row: EmptyArrayType, column: number, cellValue: string) => {
		if (!cellValue) return '-';
		let time = 0;
		if (typeof row === 'number') time = row;
		if (typeof cellValue === 'number') time = cellValue;
		return formatDate(new Date(time * 1000), 'HH:MM:SS');
	};
	// 小数格式化
	const scaleFormat = (value: string = '0', scale: number = 4) => {
		return Number.parseFloat(value).toFixed(scale);
	};
	// 小数格式化
	const scale2Format = (value: string = '0') => {
		return Number.parseFloat(value).toFixed(2);
	};
    // 千分符，默认保留两位小数
	const groupSeparator = (value: number, minimumFractionDigits: number = 2) => {
		return value.toLocaleString('en-US', {
			minimumFractionDigits: minimumFractionDigits,
			maximumFractionDigits: 2,
		});
	};

	/**
	 * 删除字符串首尾指定字符
	 * @param Str 源字符
	 * @param char 去除的指定字符
	 * @param type 类型，右边或左边，为空是替换首尾
	 */
	const trimChar =(Str:string,char:string, type:string) =>{
		if (char) {
			if (type == 'left') {
				return Str.replace(new RegExp('^\\'+char+'+', 'g'), '');
			} else if (type == 'right') {
				return Str.replace(new RegExp('\\'+char+'+$', 'g'), '');
			}
			return Str.replace(new RegExp('^\\'+char+'+|\\'+char+'+$', 'g'), '');
		}
		return Str.replace(/^\s+|\s+$/g, '');
	}
	// 点击复制文本
	const copyText = (text: string) => {
		return new Promise((resolve, reject) => {
			try {
				//复制
				toClipboard(text);
				//下面可以设置复制成功的提示框等操作
				ElMessage.success(t('message.layout.copyTextSuccess'));
				resolve(text);
			} catch (e) {
				//复制失败
				ElMessage.error(t('message.layout.copyTextError'));
				reject(e);
			}
		});
	};
	// 去掉Html标签(取前面5个字符)
	const removeHtmlSub = (value: string) => {
		var str = value.replace(/<[^>]+>/g, '');
		if (str.length > 50) return str.substring(0, 50) + '......';
		else return str;
	};
	// 去掉Html标签
	const removeHtml = (value: string) => {
		return value.replace(/<[^>]+>/g, '');
	};
	// 获取枚举描述
	const getEnumDesc = (key: any, lstEnum: any) => {
		return lstEnum.find((x: any) => x.value == key)?.describe;
	};
	// 追加query参数到url
	const appendQueryParams = (url: string, params: { [key : string]: any }) => {
		if (!params || Object.keys(params).length == 0) return url;
		const queryString = Object.keys(params).map(key => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`).join('&');
		return `${url}${url.includes('?') ? '&' : '?'}${queryString}`;
	};
	return {
		percentFormat,
		dateFormatYMD,
		dateFormatYMDHMS,
		dateFormatHMS,
		scaleFormat,
		scale2Format,
        groupSeparator,
		copyText,
		removeHtmlSub,
		removeHtml,
		getEnumDesc,
		appendQueryParams,
		trimChar,
	};
}
