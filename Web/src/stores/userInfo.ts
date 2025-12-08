import { defineStore } from 'pinia';
import { Local, Session } from '/@/utils/storage';
import Watermark from '/@/utils/watermark';
import { useThemeConfig } from '/@/stores/themeConfig';

import { getAPI } from '/@/utils/axios-utils';
import { SysAuthApi, SysConstApi, SysDictTypeApi } from '/@/api-services/api';

/**
 * 用户信息
 * @methods setUserInfos 设置用户信息
 */
export const useUserInfo = defineStore('userInfo', {
	state: (): UserInfosState => ({
		userInfos: {} as any,
		constList: [] as any,
		dictList: {} as any,
	}),
	getters: {
		// // 获取系统常量列表
		// async getSysConstList(): Promise<any[]> {
		// 	var res = await getAPI(SysConstApi).apiSysConstListGet();
		// 	this.constList = res.data.result ?? [];
		// 	return this.constList;
		// },
	},
	actions: {
		// 存储用户信息到浏览器缓存
		async setUserInfos() {
			this.userInfos = Session.get('userInfo') ?? <UserInfos>await this.getApiUserInfo();
		},

		// 存储常量信息到浏览器缓存
		async setConstList() {
			this.constList = Session.get('constList') ?? <any[]>await this.getSysConstList();
			if (!Session.get('constList')) Session.set('constList', this.constList);
		},

		// 存储字典信息到浏览器缓存
		async setDictList() {
			var dictList = await getAPI(SysDictTypeApi).apiSysDictTypeAllDictListGet().then(res => res.data.result ?? {});
			var dictListTemp = JSON.parse(JSON.stringify(dictList));

			await Promise.all(Object.keys(dictList).map(async (key) => {
				// dictList[key].forEach((da: any, index: any) => {
				// 	setDictLangMessageAsync(dictListTemp[key][index]);
				// });
				// 如果 key 以 "Enum" 结尾，则转换 value 为数字
				if (key.endsWith("Enum")) {
					dictListTemp[key].forEach((e: any) => e.value = Number(e.value));
				}
			}))
			this.dictList = dictListTemp;
		},

		// 获取当前用户信息
		getApiUserInfo() {
			return new Promise((resolve) => {
				getAPI(SysAuthApi)
					.apiSysAuthUserInfoGet()
					.then(async (res: any) => {
						if (res.data.result == null) return;
						var d = res.data.result;
						const userInfos = {
							id: d.id,
							account: d.account,
							realName: d.realName,
							phone: d.phone,
							idCardNum: d.idCardNum,
							email: d.email,
							accountType: d.accountType,
							avatar: d.avatar ?? '/upload/logo.png',
							address: d.address,
							signature: d.signature,
							orgId: d.orgId,
							orgName: d.orgName,
							posName: d.posName,
							roles: d.roleIds,
							authBtnList: d.buttons,
							tenantId: d.tenantId,
							currentTenantId: d.currentTenantId,
							langCode: d.langCode,
							time: new Date().getTime(),
						};

						// vue-next-admin 提交Id：225bce7 提交消息：admin-23.03.26:发布v2.4.32版本
						// 增加了下面代码，引起当前会话的用户信息不会刷新，如：重新提交的头像不更新，需要新开一个页面才能正确显示
						// Session.set('userInfo', userInfos);

						// 用户水印
						const storesThemeConfig = useThemeConfig();
						storesThemeConfig.themeConfig.watermarkText = d.watermarkText ?? '';
						if (storesThemeConfig.themeConfig.isWatermark) Watermark.set(storesThemeConfig.themeConfig.watermarkText);
						else Watermark.del();

						Local.remove('themeConfig');
						Local.set('themeConfig', storesThemeConfig.themeConfig);

						resolve(userInfos);
					});
			});
		},

		// 获取常量集合
		getSysConstList() {
			return new Promise((resolve) => {
				getAPI(SysConstApi)
					.apiSysConstListGet()
					.then(async (res: any) => {
						resolve(res.data.result ?? []);
					});
			});
		},

		// 根据常量类名获取常量数据
		getConstDataByTypeCode(typeCode: string) {
			return this.constList.find((item: any) => item.code === typeCode)?.data?.result || [];
		},

		// 根据常量类名和编码获取常量值
		getConstItemNameByType(typeCode: string, itemCode: string) {
			const data = this.getConstDataByTypeCode(typeCode);
			return data.find((item: any) => item.code === itemCode)?.name;
		},

		// 根据字典类型获取字典数据
		getDictDataByCode(dictTypeCode: string) {
			return this.dictList[dictTypeCode] || [];
		}
	},
});

// 处理字典国际化, 默认显示字典中的label值
// const setDictLangMessageAsync = async (dict: any) => {
// 	dict.langMessage = `message.dictType.${dict.typeCode}_${dict.value}`;
// 	const text = dict.langMessage;
// 	dict.label = text !== dict.langMessage ? text : dict.label;
// }