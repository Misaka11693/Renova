<template>
	<div class="sys-grantData-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="450px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> 授权数据范围 </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" label-position="top">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl1="24" class="mb20">
						<el-form-item label="数据范围：">
              <g-sys-dict v-model="state.ruleForm.dataScope" code="DataScopeEnum" render-as="select" placeholder="数据范围" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl1="24" v-show="state.ruleForm.dataScope === 5">
						<el-form-item label="机构列表：">
							<OrgTree ref="orgTreeRef" class="w100" :tenant-id="state.ruleForm.tenantId" />
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

<script lang="ts" setup name="sysGrantData">
import { reactive, ref } from 'vue';
import OrgTree from '/@/views/system/org/component/orgTree.vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysRoleApi } from '/@/api-services/api';

const emits = defineEmits(['handleQuery']);
const orgTreeRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} as any
});

// 打开弹窗
const openDialog = async (row: any) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
	var res = await getAPI(SysRoleApi).apiSysRoleOwnOrgListGet(row.id);
	setTimeout(() => {
		orgTreeRef.value?.setCheckedKeys(res.data.result);
	}, 100);
	state.isShowDialog = true;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 提交
const submit = async () => {
	if (state.ruleForm.dataScope === 5) state.ruleForm.orgIdList = orgTreeRef.value?.getCheckedKeys();
	await getAPI(SysRoleApi).apiSysRoleGrantDataScopePost(state.ruleForm);
	closeDialog();
};

// 导出对象
defineExpose({ openDialog });
</script>
