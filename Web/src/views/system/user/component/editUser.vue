<template>
	<div class="sys-user-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span>{{ props.title }}</span>
				</div>
			</template>
			<el-tabs v-loading="state.loading" v-model="state.selectedTabName">
				<el-tab-pane label="基础信息" class="tab-pane">
					<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="账号名称" prop="account" :rules="[{ required: true, message: '账号名称不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.account" placeholder="账号名称" :disabled="state.ruleForm.id > 0" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="昵称">
									<el-input v-model="state.ruleForm.nickName" placeholder="昵称" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="手机号码" prop="phone" :rules="[{ required: true, message: '手机号码不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.phone" placeholder="手机号码" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="真实姓名" prop="realName" :rules="[{ required: true, message: '真实姓名不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.realName" placeholder="真实姓名" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="账号类型" prop="accountType" :rules="[{ required: true, message: '账号类型不能为空', trigger: 'blur' }]">
									<g-sys-dict
										v-model="state.ruleForm.accountType"
										:on-item-filter="(data: any) => data.code != 'SuperAdmin' && (data.code == 'SysAdmin' ? AccountTypeEnum.NUMBER_999 == userInfos.accountType : true)"
										code="AccountTypeEnum"
										render-as="select"
									/>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="邮箱">
									<el-input v-model="state.ruleForm.email" placeholder="邮箱" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="语言" prop="langCode" :rules="[{ required: true, message: '语言不能为空', trigger: 'blur' }]">
									<el-select clearable filterable v-model="state.ruleForm.langCode" placeholder="请选择语言">
										<el-option v-for="(item, index) in state.languages" :key="index" :value="item.code" :label="item.label" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="个性化首页" prop="homepage">
									<el-input v-model="state.ruleForm.homepage" placeholder="个性化首页" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb5">
								<el-form-item label="排序">
									<el-input-number v-model="state.ruleForm.orderNo" placeholder="排序" class="w100" />
								</el-form-item>
							</el-col>
							<el-divider border-style="dashed" content-position="center">
								<div style="color: #b1b3b8">机构组织</div>
							</el-divider>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="所属机构" prop="orgId" :rules="[{ required: true, message: '所属机构不能为空', trigger: 'blur' }]">
									<el-cascader :options="state.orgTreeData" :props="cascaderProps" placeholder="所属机构" clearable filterable class="w100" v-model="state.ruleForm.orgId">
										<template #default="{ node, data }">
											<span>{{ data.name }}</span>
											<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
										</template>
									</el-cascader>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="职位" prop="posId" :rules="[{ required: true, message: '职位名称不能为空', trigger: 'blur' }]">
									<el-select v-model="state.ruleForm.posId" placeholder="职位" class="w100">
										<el-option v-for="d in state.posData" :key="d.id" :label="d.name" :value="d.id" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="工号">
									<el-input v-model="state.ruleForm.jobNum" placeholder="工号" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="域账号">
									<el-input v-model="state.ruleForm.domainAccount" placeholder="域账号" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="入职日期">
									<el-date-picker v-model="state.ruleForm.joinDate" type="date" placeholder="入职日期" format="YYYY-MM-DD" value-format="YYYY-MM-DD" class="w100" />
								</el-form-item>
							</el-col>
							<el-divider border-style="dashed" content-position="center">
								<div style="color: #b1b3b8">附属机构</div>
							</el-divider>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-button icon="ele-Plus" type="primary" text plain @click="addExtOrgRow"> 增加附属机构 </el-button>
								<span style="font-size: 12px; color: gray; padding-left: 5px"> 具有相应组织机构的数据权限 </span>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="unique-box">
								<template v-if="state.ruleForm.extOrgIdList != undefined && state.ruleForm.extOrgIdList.length > 0">
                                    <div v-for="(v, k) in state.ruleForm.extOrgIdList" :key="k" class="unique-line">
                                        <el-form-item label="机构" label-width="55" :prop="`extOrgIdList[${k}].orgId`" :rules="[{ required: true, message: `机构不能为空`, trigger: 'blur' }]">
											<el-cascader :options="props.orgTreeData" :props="cascaderProps" placeholder="机构组织" clearable filterable class="w100" v-model="state.ruleForm.extOrgIdList[k].orgId">
												<template #default="{ node, data }">
													<span>{{ data.name }}</span>
													<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
												</template>
											</el-cascader>
										</el-form-item>
                                        <el-form-item label="职位" label-width="55" :prop="`extOrgIdList[${k}].posId`" :rules="[{ required: true, message: `职位不能为空`, trigger: 'blur' }]">
											<el-select v-model="state.ruleForm.extOrgIdList[k].posId" placeholder="职位名称" class="w100">
												<el-option v-for="d in state.posData" :key="d.id" :label="d.name" :value="d.id" />
											</el-select>
										</el-form-item>
                                        <div class="delete-btn">
                                            <el-button icon="ele-Delete" type="danger" circle plain size="small" @click="deleteExtOrgRow(k)" />
                                        </div>
                                    </div>
								</template>
								<el-empty :image-size="50" style="padding: 0px;" v-else></el-empty>
							</el-col>
						</el-row>
					</el-form>
				</el-tab-pane>
				<el-tab-pane label="角色授权" class="tab-pane">
					<el-transfer :data="state.roleData" :props="{ key: 'id', label: 'name' }" v-model="state.ruleForm.roleIdList" :titles="['未授权', '已授权']"></el-transfer>
				</el-tab-pane>
				<el-tab-pane label="档案信息" class="tab-pane">
					<el-form :model="state.ruleForm" label-width="auto">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="证件类型" prop="cardType">
									<g-sys-dict v-model="state.ruleForm.cardType" code="CardTypeEnum" render-as="select" placeholder="证件类型" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="证件号码">
									<el-input v-model="state.ruleForm.idCardNum" placeholder="证件号码" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="出生日期" prop="birthday">
									<el-date-picker v-model="state.ruleForm.birthday" type="date" placeholder="出生日期" format="YYYY-MM-DD" value-format="YYYY-MM-DD" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="性别">
									<g-sys-dict v-model="state.ruleForm.sex" code="GenderEnum" render-as="radio" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb5">
								<el-form-item label="年龄">
									<el-input-number v-model="state.ruleForm.age" placeholder="年龄" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="民族">
									<g-sys-dict v-model="state.ruleForm.nation" code="NationEnum" render-as="select" placeholder="民族" class="w100" clearable/>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="地址">
									<el-input v-model="state.ruleForm.address" placeholder="地址" clearable type="textarea" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="毕业学校">
									<el-input v-model="state.ruleForm.college" placeholder="毕业学校" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="文化程度">
									<g-sys-dict v-model="state.ruleForm.cultureLevel" code="CultureLevelEnum" render-as="select" placeholder="文化程度" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="政治面貌">
									<el-input v-model="state.ruleForm.politicalOutlook" placeholder="政治面貌" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="办公电话">
									<el-input v-model="state.ruleForm.officePhone" placeholder="办公电话" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="紧急联系人">
									<el-input v-model="state.ruleForm.emergencyContact" placeholder="紧急联系人" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="联系人电话">
									<el-input v-model="state.ruleForm.emergencyPhone" placeholder="联系人电话" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="联系人地址">
									<el-input v-model="state.ruleForm.emergencyAddress" placeholder="联系人地址" clearable type="textarea" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="备注">
									<el-input v-model="state.ruleForm.remark" placeholder="备注" clearable type="textarea" />
								</el-form-item>
							</el-col>
						</el-row>
					</el-form>
				</el-tab-pane>
			</el-tabs>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditUser">
import { onMounted, reactive, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useUserInfo } from '/@/stores/userInfo';
import { getAPI } from '/@/utils/axios-utils';
import { SysPosApi, SysRoleApi, SysUserApi } from '/@/api-services/api';
import {AccountTypeEnum, RoleOutput, OrgTreeOutput, SysPos, UpdateUserInput} from '/@/api-services/models';
import { useLangStore } from '/@/stores/useLangStore';
const langStore = useLangStore();

const props = defineProps({
	title: String,
	orgTreeData: Array<OrgTreeOutput>,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const storesUserInfo = useUserInfo();
const { userInfos } = storeToRefs(storesUserInfo);
const state = reactive({
	loading: false,
	isShowDialog: false,
	selectedTabName: '0', // 选中的 tab 页
	orgTreeData: [] as Array<OrgTreeOutput>,
	ruleForm: {} as UpdateUserInput,
	posData: [] as Array<SysPos>, // 职位数据
	roleData: [] as Array<RoleOutput>, // 角色数据
	languages: [] as any[], // 语言数据
});
// 级联选择器配置选项
const cascaderProps = { checkStrictly: true, emitPath: false, value: 'id', label: 'name', expandTrigger: 'hover' };

onMounted(async () => {
	state.loading = true;
	var res = await getAPI(SysPosApi).apiSysPosListGet();
	state.posData = res.data.result ?? [];
	var res1 = await getAPI(SysRoleApi).apiSysRoleListGet();
	state.roleData = res1.data.result ?? [];
	if (langStore.languages.length === 0) {
        await langStore.loadLanguages();
    }
	state.languages = langStore.languages;
	state.loading = false;
});

// 打开弹窗
const openDialog = async (row: any) => {
	state.orgTreeData = (row.tenantId ? props.orgTreeData?.filter((e) => e.tenantId === row.tenantId) : props.orgTreeData) ?? [];
	state.posData = (row.tenantId ? state.posData?.filter((e) => e.tenantId === row.tenantId) : state.posData) ?? [];
	state.roleData = (row.tenantId ? state.roleData?.filter((e) => e.tenantId === row.tenantId) : state.roleData) ?? [];
	ruleFormRef.value?.resetFields();
	state.selectedTabName = '0'; // 重置为第一个 tab 页
	state.ruleForm = JSON.parse(JSON.stringify(row));
	if (row.id != undefined) {
		var resRole = await getAPI(SysUserApi).apiSysUserOwnRoleListUserIdGet(row.id);
		state.ruleForm.roleIdList = resRole.data.result;
		var resExtOrg = await getAPI(SysUserApi).apiSysUserOwnExtOrgListUserIdGet(row.id);
		state.ruleForm.extOrgIdList = resExtOrg.data.result;
	} else state.ruleForm.accountType = 777; // 默认普通账号类型
	state.isShowDialog = true;
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
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
			await getAPI(SysUserApi).apiSysUserUpdatePost(state.ruleForm);
		} else {
			await getAPI(SysUserApi).apiSysUserAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

// 增加附属机构行
const addExtOrgRow = () => {
	if (state.ruleForm.extOrgIdList == undefined) state.ruleForm.extOrgIdList = [];
	state.ruleForm.extOrgIdList?.push({});
};

// 删除附属机构行
const deleteExtOrgRow = (k: number) => {
	state.ruleForm.extOrgIdList?.splice(k, 1);
};

// 导出对象
defineExpose({ openDialog });
</script>

<style lang="scss" scoped>
.tab-pane {
    padding: 0 10px;
    height: 570px;
    overflow: hidden auto;

    .el-transfer {
        margin: 0 auto;
        width: fit-content;
        height: 100%;

        :deep(.el-transfer-panel) {
            height: 100%;
        }

        --el-transfer-panel-body-height: calc(100% - 40px);
    }

    .unique-box {
        display: grid;
        gap: 20px;
    }
    .unique-line {
        display: flex;
        gap: 20px;
        
        .el-form-item {
            margin-bottom: 0;
            flex: 1;
        }

        .delete-btn {
            align-self: center;
            width: 24px;
            margin-left: -10px;
        }
    }
}
</style>