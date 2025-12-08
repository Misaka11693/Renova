<template>
	<div class="sys-tenant-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit />
					</el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-tabs v-loading="state.loading" v-model="state.selectedTabName">
					<el-tab-pane label="基本信息" style="height: 450px; overflow-y: auto; overflow-x: hidden">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="租户类型"
									:rules="[{ required: true, message: '租户类型不能为空', trigger: 'blur' }]">
									<g-sys-dict v-model="state.ruleForm.tenantType" code="TenantTypeEnum" render-as="radio"
										:disabled="state.ruleForm.id != undefined" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="租户名称" prop="name"
									:rules="[{ required: true, message: '租户名称不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.name" placeholder="租户名称" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="租管账号" prop="adminAccount"
									:rules="[{ required: true, message: '租管账号不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.adminAccount" placeholder="租管账号" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="电话" prop="phone"
									:rules="[{ required: true, message: '电话号码不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.phone" placeholder="电话" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="数据库类型">
									<el-select v-model="state.ruleForm.dbType" placeholder="数据库类型" clearable class="w100"
										:disabled="state.ruleForm.tenantType == 0 && state.ruleForm.tenantType != undefined">
										<el-option label="MySql" :value="0" />
										<el-option label="SqlServer" :value="1" />
										<el-option label="Sqlite" :value="2" />
										<el-option label="Oracle" :value="3" />
										<el-option label="PostgreSQL" :value="4" />
										<el-option label="Dm" :value="5" />
										<el-option label="Kdbndp" :value="6" />
										<el-option label="Oscar" :value="7" />
										<el-option label="MySqlConnector" :value="8" />
										<el-option label="Access" :value="9" />
										<el-option label="OpenGauss" :value="10" />
										<el-option label="QuestDB" :value="11" />
										<el-option label="HG" :value="12" />
										<el-option label="ClickHouse" :value="13" />
										<el-option label="GBase" :value="14" />
										<el-option label="Odbc" :value="'15'" />
										<el-option label="OceanBaseForOracle" :value="'16'" />
										<el-option label="TDengine" :value="'17'" />
										<el-option label="GaussDB" :value="'18'" />
										<el-option label="OceanBase" :value="'19'" />
										<el-option label="Tidb" :value="'20'" />
										<el-option label="Vastbase" :value="'21'" />
										<el-option label="PolarDB" :value="'22'" />
										<el-option label="Doris" :value="'23'" />
										<el-option label="Custom" :value="900" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="主机host">
									<el-input v-model="state.ruleForm.host" placeholder="例：gitee.com" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="连接字符串">
									<el-input v-model="state.ruleForm.connection" placeholder="连接字符串" clearable
										type="textarea"
										:disabled="state.ruleForm.tenantType == 0 && state.ruleForm.tenantType != undefined" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="从库连接串">
									<el-input v-model="state.ruleForm.slaveConnections"
										placeholder="格式：[{'HitRate':10, 'ConnectionString':'xxx'},{'HitRate':10, 'ConnectionString':'xxx'}]"
										clearable type="textarea"
										:disabled="state.ruleForm.tenantType == 0 && state.ruleForm.tenantType != undefined" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="邮箱">
									<el-input v-model="state.ruleForm.email" placeholder="邮箱" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="排序">
									<el-input-number v-model="state.ruleForm.orderNo" placeholder="排序" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="备注">
									<el-input v-model="state.ruleForm.remark" placeholder="请输入备注内容" clearable
										type="textarea" />
								</el-form-item>
							</el-col>
						</el-row>
					</el-tab-pane>
					<el-tab-pane label="站点信息" style="height: 450px; overflow: auto; overflow-x: hidden"
						v-if="state.ruleForm.host?.trim()">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="Logo" prop="logo"
									:rules="[{ required: true, message: '应用Logo不能为空', trigger: 'blur' }]">
									<el-upload ref="uploadRef" class="avatar-uploader" :showFileList="false"
										:autoUpload="false" accept=".jpg,.png,.svg" action :limit="1"
										:onChange="handleUploadChange">
										<img v-if="state.ruleForm.logo" :src="state.ruleForm.logo" class="avatar"
											style="max-width: 100px; max-height: 100px; object-fit: contain" />
										<SvgIcon v-else class="avatar-uploader-icon" name="ele-Plus" :size="28" />
									</el-upload>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="标题" prop="title"
									:rules="[{ required: true, message: '标题不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.title" placeholder="应用标题" maxlength="32" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="副标题" prop="viceTitle"
									:rules="[{ required: true, message: '副标题不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.viceTitle" placeholder="应用副标题" maxlength="32"
										clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="副标题描述" prop="viceDesc"
									:rules="[{ required: true, message: '副标题描述不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.viceDesc" placeholder="应用副标题描述" maxlength="64"
										clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="版权信息" prop="copyright"
									:rules="[{ required: true, message: '版权信息不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.copyright" placeholder="版权信息" maxlength="64"
										clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="备案号" prop="icp">
									<el-input v-model="state.ruleForm.icp" placeholder="例：省ICP备12345678号" maxlength="32" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="ICP地址" prop="icpUrl"
									:rules="[{ required: state.ruleForm.icp, message: 'ICP地址不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.icpUrl" placeholder="例：https://beian.miit.gov.cn" maxlength="32"
										clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="水印" prop="watermark">
									<el-input v-model="state.ruleForm.watermark" placeholder="如果此处留空，则水印功能将被禁用"
										maxlength="32" clearable />
								</el-form-item>
							</el-col>
						</el-row>
					</el-tab-pane>
				</el-tabs>
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

<script lang="ts" setup name="sysEditTenant">
import { reactive, ref } from 'vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysTenantApi, SysUserRegWayApi } from '/@/api-services/api';
import { UpdateTenantInput } from '/@/api-services/models';
import { UploadInstance } from 'element-plus';
import { fileToBase64 } from '/@/utils/base64Conver';
import GSysDict from '/@/components/sysDict/sysDict.vue';

const props = defineProps({
	title: String,
});
const emits = defineEmits(['handleQuery']);
const uploadRef = ref<UploadInstance>();
const ruleFormRef = ref();
const state = reactive({
	loading: false,
	selectedTabName: '0',
	isShowDialog: false,
	file: undefined as any,
	regWayData: [] as Array<any>,
	ruleForm: {} as UpdateTenantInput,
});

// 通过onChange方法获得文件列表
const handleUploadChange = (file: any) => {
	uploadRef.value!.clearFiles();
	state.file = file;
	state.ruleForm.logo = URL.createObjectURL(state.file.raw); // 显示预览logo
};

// 打开弹窗
const openDialog = async (row: any) => {
	state.selectedTabName = '0';
	ruleFormRef.value?.resetFields();
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.ruleForm.copyright ??= `Copyright \u00a9 ${new Date().getFullYear()}-present xxxxx All rights reserved.`;
	state.isShowDialog = true;
	state.regWayData = await getAPI(SysUserRegWayApi).apiSysUserRegWayListPost({ tenantId: row.id }).then((res) => res.data.result ?? []);
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = async () => {
	// 如果有选择图标，则转换为 base64
	if (state.file) {
		state.ruleForm.logoBase64 = (await fileToBase64(state.file.raw)) as string;
		state.ruleForm.logoFileName = state.file.raw.name;
	}
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		state.loading = true;
		try {
			if (state.ruleForm.enableReg != 1) state.ruleForm.regWayId = undefined;
			if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
				await getAPI(SysTenantApi).apiSysTenantUpdatePost(state.ruleForm);
			} else {
				await getAPI(SysTenantApi).apiSysTenantAddPost(state.ruleForm);
			}
			closeDialog();
		} finally {
			state.loading = false;
		}
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
