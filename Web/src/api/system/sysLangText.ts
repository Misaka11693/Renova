import {useBaseApi} from '/@/api/base';

// 翻译接口服务
export const useSysLangTextApi = () => {
	const baseApi = useBaseApi("sysLangText");
	return {
		// 分页查询翻译
		page: baseApi.page,
		// 查看翻译详细
		detail: baseApi.detail,
		// 新增翻译
		add: baseApi.add,
		// 更新翻译
		update: baseApi.update,
		// 删除翻译
		delete: baseApi.delete,
		// 批量删除翻译
		batchDelete: baseApi.batchDelete,
		// 导出翻译数据
		exportData: baseApi.exportData,
		// 导入翻译数据
		importData: baseApi.importData,
		// 下载翻译数据导入模板
		downloadTemplate: baseApi.downloadTemplate,
	}
}

// 翻译实体
export interface SysLangText {
	// 主键Id
	id: number;
	// 所属实体名
	entityName?: string;
	// 所属实体ID
	entityId?: number;
	// 字段名
	fieldName?: string;
	// 语言代码
	langCode?: string;
	// 翻译内容
	content?: string;
	// 创建时间
	createTime: string;
	// 更新时间
	updateTime: string;
	// 创建者Id
	createUserId: number;
	// 创建者姓名
	createUserName: string;
	// 修改者Id
	updateUserId: number;
	// 修改者姓名
	updateUserName: string;
}