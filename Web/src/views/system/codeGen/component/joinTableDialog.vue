<template>
	<div class="sys-codeGenTree-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{state.dialogTitle}} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="库定位器" prop="fkConfigId" :rules="[{ required: true, message: '库不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.fkConfigId" placeholder="库名" filterable clearable @change="DbChanged()" class="w100">
								<el-option v-for="item in state.dbData" :key="item.configId" :label="item.dbNickName" :value="item.configId" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="数据库表" prop="fkTableName" :rules="[{ required: true, message: '数据表不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.fkTableName" class="w100" filterable clearable @change="TableChanged()">
								<el-option v-for="item in state.tableData" :key="item.entityName" :label="item.tableName + ' [' + item.tableComment + ']'" :value="item.tableName" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="显示字段" prop="fkDisplayColumnList" :rules="[{ required: true, message: '显示字段不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.fkDisplayColumnList" multiple filterable clearable class="w100">
								<el-option v-for="item in state.columnData" :key="item.propertyName" :label="item.propertyName + ' [' + item.columnComment + ']'" :value="item.propertyName" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="值&ensp;字&ensp;段" prop="fkLinkColumnName" :rules="[{ required: true, message: '值字段不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.fkLinkColumnName" filterable clearable class="w100">
								<el-option v-for="item in state.columnData" :key="item.propertyName" :label="item.propertyName + ' [' + item.columnComment + ']'" :value="item.propertyName" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20" v-if="state.ruleForm.effectType == 'ApiTreeSelector'">
						<el-form-item label="父级字段" prop="pidColumn" :rules="[{ required: true, message: '父级字段不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.pidColumn" filterable clearable class="w100">
								<el-option v-for="item in state.columnData" :key="item.propertyName" :label="item.propertyName + ' [' + item.columnComment + ']'" :value="item.propertyName" />
							</el-select>
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysCodeGenTree">
import { reactive, ref } from 'vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysCodeGenApi } from '/@/api-services/api';

const emits = defineEmits(['submitRefreshFk']);
const ruleFormRef = ref();
const state = reactive({
  dialogTitle: '' as string,
	isShowDialog: false,
	ruleForm: {} as any,
	dbData: [] as any,
	tableData: [] as any,
	columnData: [] as any,
});

const DbChanged = async () => {
	state.tableData = [];
	state.columnData = [];
	await getTableInfoList();
};

const TableChanged = async () => {
	state.columnData = [];
	await getColumnInfoList();
  state.ruleForm.pidColumn = undefined;
	state.ruleForm.fkDisplayColumnList = undefined;
  state.ruleForm.fkLinkColumnName = state.columnData.find(x => "True" === x.columnKey)?.columnName;
};

const getDbList = async () => {
  state.dbData = await getAPI(SysCodeGenApi).apiSysCodeGenDatabaseListGet().then(res => res.data.result ?? []);
};

const getTableInfoList = async () => {
	if (!state.ruleForm.fkConfigId) return;
  state.tableData = await getAPI(SysCodeGenApi)
      .apiSysCodeGenTableListConfigIdGet(state.ruleForm.fkConfigId)
      .then(res => res.data.result ?? []);
};

const getColumnInfoList = async () => {
	if (!state.ruleForm.fkConfigId || !state.ruleForm.fkTableName) return;
  state.columnData = await getAPI(SysCodeGenApi)
      .apiSysCodeGenColumnListByTableNameTableNameConfigIdGet(state.ruleForm.fkTableName, state.ruleForm.fkConfigId)
      .then(res => res.data.result ?? []);
};

// 打开弹窗
const openDialog = async (row: any, title: string) => {
  await getDbList();
  state.dialogTitle = title;
  state.isShowDialog = true;
  state.ruleForm = JSON.parse(JSON.stringify(row));
	if (row.fkConfigId) {
		await DbChanged();
    await TableChanged();
    state.ruleForm.pidColumn = row.pidColumn;
    state.ruleForm.fkLinkColumnName = row.fkLinkColumnName;
    state.ruleForm.fkDisplayColumnList = row.fkDisplayColumnList;
	}
};

// 关闭弹窗
const closeDialog = () => {
  state.ruleForm.fkColumnNetType = state.columnData.find(x => x.columnName == state.ruleForm.fkLinkColumnName)?.netType;
  state.ruleForm.fkEntityName = state.tableData.find(x => x.tableName == state.ruleForm.fkTableName)?.entityName;
	if (state.ruleForm.effectType != 'ApiTreeSelector') state.ruleForm.pidColumn = null;
  emits('submitRefreshFk', state.ruleForm);
	cancel();
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
	state.dbData.value = [];
	state.tableData.value = [];
	state.columnData.value = [];
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
