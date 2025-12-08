<template>
	<div class="sys-tenant-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="方案名称" prop="name" :rules="[{ required: true, message: '方案名称不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.name" placeholder="方案名称" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="账户类型" prop="posId" :rules="[{ required: true, message: '账户类型不能为空', trigger: 'blur' }]">
							<g-sys-dict
									v-model="state.ruleForm.accountType"
									:on-item-filter="(data: any) => !['SuperAdmin','SysAdmin'].includes(data.name)"
									code="AccountTypeEnum"
									render-as="select"
							/>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="绑定角色" prop="roleId" :rules="[{ required: true, message: '角色不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.roleId" placeholder="绑定角色" clearable class="w100">
								<el-option :label="item.name" :value="item.id" v-for="(item, index) in state.roleData" :key="index" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="绑定机构" prop="orgId" :rules="[{ required: true, message: '机构不能为空', trigger: 'blur' }]">
							<el-cascader :options="state.orgData" :props="cascaderConfig" v-model="state.ruleForm.orgId" placeholder="绑定机构" clearable filterable class="w100" >
								<template #default="{ node, data }">
									<span>{{ data.name }}</span>
									<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
								</template>
							</el-cascader>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="绑定职位" prop="posId" :rules="[{ required: true, message: '职位不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.posId" placeholder="绑定职位" clearable class="w100">
								<el-option :label="item.name" :value="item.id" v-for="(item, index) in state.posData" :key="index" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="排序">
							<el-input-number v-model="state.ruleForm.orderNo" placeholder="排序" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注内容" clearable type="textarea" />
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="() => state.isShowDialog = false">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditTenant">
import { onMounted, reactive, ref } from 'vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysOrgApi, SysPosApi, SysRoleApi, SysUserRegWayApi } from '/@/api-services/api';
import { OrgTreeOutput, RoleOutput, SysPos, UpdateUserRegWayInput } from '/@/api-services/models';

const props = defineProps({
	title: String,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	loading: false,
	selectedTabName: '0',
	isShowDialog: false,
	file: undefined as any,
	ruleForm: {} as UpdateUserRegWayInput,
	orgData: [] as Array<OrgTreeOutput>,
	posData: [] as Array<SysPos>, // 职位数据
	roleData: [] as Array<RoleOutput>, // 角色数据
});

onMounted(async () => {
	state.loading = true;
	state.posData = await getAPI(SysPosApi).apiSysPosListGet().then(res => res.data.result ?? []);
	state.roleData = await getAPI(SysRoleApi).apiSysRoleListGet().then(res => res.data.result ?? []);
	state.orgData = await getAPI(SysOrgApi).apiSysOrgTreeGet(0).then(res => res.data.result ?? []);
	state.loading = false;
});

// 级联选择器配置选项
const cascaderConfig = {
	checkStrictly: true,
	emitPath: false,
	value: 'id',
	label: 'name',
	expandTrigger: 'hover'
};

// 打开弹窗
const openDialog = async (row: any) => {
	state.roleData = (row?.tenantId ? state.roleData?.filter((e) => e.tenantId === row.tenantId) : state.roleData) ?? [];
	state.posData = (row?.tenantId ? state.posData?.filter((e) => e.tenantId === row.tenantId) : state.posData) ?? [];
	state.orgData = (row?.tenantId ? state.orgData?.filter((e) => e.tenantId === row.tenantId) : state.orgData) ?? [];
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 提交
const submit = async () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		if (state.ruleForm.id) {
			await getAPI(SysUserRegWayApi).apiSysUserRegWayUpdatePost(state.ruleForm);
		} else {
			await getAPI(SysUserRegWayApi).apiSysUserRegWayAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
