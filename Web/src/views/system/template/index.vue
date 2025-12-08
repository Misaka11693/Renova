<template>
	<div class="sys-config-container">
		<el-card shadow="hover" :body-style="{ paddingBottom: 5 }">
			<TableSearch :search="tb.tableData.search" @search="onSearch" />
		</el-card>
		<el-card class="full-table" shadow="hover" style="margin-top: 5px">
			<Table ref="tableRef" v-bind="tb.tableData" :getData="getData" @sortHeader="onSortHeader" border>
				<template #command>
					<el-button type="primary" icon="ele-Plus" @click="openAddTemplate" v-auth="'sysConfig:add'"> 新增 </el-button>
				</template>
				<template #type="scope">
					<g-sys-dict v-model="scope.row.type" code="TemplateTypeEnum" />
				</template>
				<template #remark="scope">
					<ModifyRecord :data="scope.row" />
				</template>
				<template #action="scope">
					<el-button icon="ele-Edit" size="small" text type="primary" @click="openEditTemplate(scope.row)" v-auth="'sysConfig:update'"> 编辑 </el-button>
					<el-button icon="ele-Delete" size="small" text type="danger" @click="delTemplate(scope.row)" v-auth="'sysConfig:delete'" :disabled="scope.row.sysFlag === 1"> 删除 </el-button>
				</template>
			</Table>
		</el-card>
		<EditTemplate ref="editTemplateRef" :title="state.editTemplateTitle" :groupList="state.groupList" @updateData="updateData" />
	</div>
</template>

<script lang="ts" setup name="sysConfig">
import { onMounted, reactive, ref, defineAsyncComponent, nextTick } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import { getAPI } from '/@/utils/axios-utils';
import { SysTemplateApi } from "/@/api-services";
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import EditTemplate from './component/editTemplate.vue';
import GSysDict from "/@/components/sysDict/sysDict.vue";

// 引入组件
const TableSearch = defineAsyncComponent(() => import('/@/components/table/search.vue'));
const Table = defineAsyncComponent(() => import('/@/components/table/index.vue'));
const editTemplateRef = ref<InstanceType<typeof EditTemplate>>();
const tableRef = ref<RefType>();

const state = reactive({
	editTemplateTitle: '',
	selectList: [] as EmptyObjectType[],
	groupList: [] as Array<String>,
});

const tb = reactive<TableDemoState>({
	tableData: {
		// 表头内容（必传，注意格式）
		columns: [
			{ prop: 'name', minWidth: 150, label: '模板名称', headerAlign: 'center', sortable: 'custom', isCheck: true, hideCheck: true },
			{ prop: 'code', minWidth: 150, label: '模板编码', headerAlign: 'center', toolTip: true, sortable: 'custom', isCheck: true },
			{ prop: 'type', width: 120, label: '模板类型', align: 'center', sortable: 'custom', isCheck: true },
			{ prop: 'groupName', width: 120, label: '分组编码', align: 'center', sortable: 'custom', isCheck: true },
			{ prop: 'orderNo', width: 80, label: '排序', align: 'center', sortable: 'custom', isCheck: true },
			{ prop: 'remark', width: 100, label: '修改记录', align: 'center', headerAlign: 'center', showOverflowTooltip: true, isCheck: true },
			{ prop: 'action', width: 140, label: '操作', type: 'action', align: 'center', isCheck: true, fixed: 'right', hideCheck: true },
		],
		// 配置项（必传）
		config: {
			isStripe: true, // 是否显示表格斑马纹
			isBorder: false, // 是否显示表格边框
			isSerialNo: true, // 是否显示表格序号
			isSelection: false, // 是否勾选表格多选
			showSelection: false, //是否显示表格多选
			pageSize: 50, // 每页条数
			hideExport: true, //是否隐藏导出按钮
		},
		// 搜索表单，动态生成（传空数组时，将不显示搜索，type有3种类型：input,date,select）
		search: [
            { label: '名称', prop: 'name', placeholder: '搜索模板名称', required: false, type: 'input' },
			{ label: '编码', prop: 'code', placeholder: '搜索模板编码', required: false, type: 'input' },
			{ label: '类型', prop: 'type', placeholder: '搜索模板类型', required: false, type: 'select', dictCode: 'TemplateTypeEnum' },
		],
		param: {},
		defaultSort: {
			prop: 'orderNo',
			order: 'ascending',
		},
	},
});
const getData = (param: any) => {
	return getAPI(SysTemplateApi)
		.apiSysTemplatePagePost(param)
		.then((res) => {
			return res.data;
		});
};

// 拖动显示列排序回调
const onSortHeader = (data: object[]) => {
	tb.tableData.columns = data;
};

// 搜索点击时表单回调
const onSearch = (data: EmptyObjectType) => {
	tb.tableData.param = Object.assign({}, tb.tableData.param, { ...data });
	nextTick(() => {
		tableRef.value.pageReset();
	});
};

// 获取分组列表
const getGroupList = async () => {
	const res = await getAPI(SysTemplateApi).apiSysTemplateGroupListGet();
	const groupSearch = {
		label: '分组编码',
		prop: 'groupName',
		placeholder: '请选择',
		required: false,
		type: 'select',
		options: [],
	} as TableSearchType;
	state.groupList = res.data.result ?? [];
	res.data.result?.forEach((item) => {
		groupSearch.options?.push({ label: item, value: item });
	});
	let group = tb.tableData.search.filter((item) => {
		return item.prop == 'groupCode';
	});
	if (group.length == 0) {
		tb.tableData.search.push(groupSearch);
	} else {
		group[0] = groupSearch;
	}
};

onMounted(async () => {
	getGroupList();
});

// 更新数据
const updateData = () => {
	tableRef.value.handleList();
	getGroupList();
};

// 打开新增页面
const openAddTemplate = () => {
	state.editTemplateTitle = '添加模板';
	editTemplateRef.value?.openDialog({ type: 1, orderNo: 100 });
};

// 打开编辑页面
const openEditTemplate = (row: any) => {
	state.editTemplateTitle = '编辑模板';
	editTemplateRef.value?.openDialog(row);
};

// 删除
const delTemplate = (row: any) => {
	ElMessageBox.confirm(`确定删除模板：【${row.name}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	}).then(async () => {
			await getAPI(SysTemplateApi).apiSysTemplateDeletePost({ id: row.id });
			tableRef.value.handleList();
			ElMessage.success('删除成功');
	}).catch(() => {});
};
</script>
