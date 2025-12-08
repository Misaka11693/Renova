import { defineStore } from 'pinia';
import { getAPI } from '/@/utils/axios-utils';
import { SysLangApi } from '/@/api-services/api';

export const useLangStore = defineStore('lang', {
	state: () => ({
		languages: [] as any[],
	}),
	actions: {
		async loadLanguages() {
			if (this.languages.length === 0) {
				const res = await getAPI(SysLangApi).apiSysLangDropdownDataPost();
				this.languages = res.data.result ?? [];
			}
		},
	},
});
