/**
 * 判断两数组字符串是否相同（用于按钮权限验证），数组字符串中存在相同时会自动去重（按钮权限标识不会重复）
 * @param news 新数据
 * @param old 源数据
 * @returns 两数组相同返回 `true`，反之则反
 */
export function judgementSameArr(newArr: unknown[] | string[], oldArr: string[]): boolean {
	const news = removeDuplicate(newArr);
	const olds = removeDuplicate(oldArr);
	let count = 0;
	const leng = news.length;
	for (let i in olds) {
		for (let j in news) {
			if (olds[i] === news[j]) count++;
		}
	}
	return count === leng ? true : false;
}

/**
 * 判断两个对象是否相同
 * @param a 要比较的对象一
 * @param b 要比较的对象二
 * @returns 相同返回 true，反之则反
 */
export function isObjectValueEqual<T extends Record<string, any>>(a: T, b: T): boolean {
	if (!a || !b) return false;
	let aProps = Object.getOwnPropertyNames(a);
	let bProps = Object.getOwnPropertyNames(b);
	if (aProps.length != bProps.length) return false;
	for (let i = 0; i < aProps.length; i++) {
		let propName = aProps[i];
		let propA = a[propName];
		let propB = b[propName];
		if (!b.hasOwnProperty(propName)) return false;
		if (propA instanceof Object) {
			if (!isObjectValueEqual(propA, propB)) return false;
		} else if (propA !== propB) {
			return false;
		}
	}
	return true;
}

/**
 * 原始实现：数组、数组对象去重
 * @param arr 数组内容
 * @param attr 需要去重的键值（数组对象）
 * @returns
 */
/*
export function removeDuplicate(arr: EmptyArrayType, attr?: string) {
	if (!Object.keys(arr).length) {
		return arr;
	} else {
		if (attr) {
			const obj: EmptyObjectType = {};
			return arr.reduce((cur: EmptyArrayType[], item: EmptyArrayType) => {
				obj[item[attr]] ? '' : (obj[item[attr]] = true && item[attr] && cur.push(item));
				return cur;
			}, []);
		} else {
			return [...new Set(arr)];
		}
	}
}
*/
/**
 * 优化后实现：数组、数组对象去重
 * 支持普通数组和对象数组去重，类型安全，且兼容原有所有调用方式
 * @param arr 数组内容
 * @param attr 需要去重的键值（数组对象）
 * @returns
 */
export function removeDuplicate<T>(arr: T[], attr?: string): T[] {
	if (!arr.length) {
		return arr;
	} else {
		if (attr) {
			const obj: Record<string, boolean> = {};
			return arr.reduce((cur: T[], item: T) => {
				const key = (item as any)[attr];
				if (key && !obj[key]) {
					obj[key] = true;
					cur.push(item);
				}
				return cur;
			}, []);
		} else {
			return [...new Set(arr)];
		}
	}
}

/* 数组、对象深拷贝
 * @param value 需要拷贝内容
 * @returns
 */
export const clone = <T>(value: T): T => {
	if (!value) return value;

	// 数组
	if (Array.isArray(value)) return value.map((item) => clone(item)) as unknown as T;

	// 普通对象
	if (typeof value === 'object') {
		return Object.fromEntries(
			Object.entries(value).map(([k, v]: [string, any]) => {
				return [k, clone(v)];
			})
		) as unknown as T;
	}
	// 基本类型
	return value;
};
