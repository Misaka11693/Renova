<script lang="ts" name="sysLang" setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage } from "element-plus";
import type { FormRules } from "element-plus";
import { formatDate } from '/@/utils/formatTime';
import { getAPI } from '/@/utils/axios-utils';
import { SysLangApi } from '/@/api-services/api';

//父级传递来的函数，用于回调
const emit = defineEmits(["reloadTable"]);
const ruleFormRef = ref();

const state = reactive({
	title: '',
	loading: false,
	showDialog: false,
	ruleForm: {} as any,
	stores: {},
	dropdownData: {} as any,
});

// 自行添加其他规则
const rules = ref<FormRules>({
	name: [{ required: true, message: '请选择语言名称！', trigger: 'blur', },],
	code: [{ required: true, message: '请选择语言代码！', trigger: 'blur', },],
	isoCode: [{ required: true, message: '请选择ISO 语言代码！', trigger: 'blur', },],
	urlCode: [{ required: true, message: '请选择URL 语言代码！', trigger: 'blur', },],
	direction: [{ required: true, message: '请选择书写方向！', trigger: 'blur', },],
	dateFormat: [{ required: true, message: '请选择日期格式！', trigger: 'blur', },],
	timeFormat: [{ required: true, message: '请选择时间格式！', trigger: 'blur', },],
	weekStart: [{ required: true, message: '请选择每周起始日！', trigger: 'blur', },],
	grouping: [{ required: true, message: '请选择分组符号！', trigger: 'blur', },],
	decimalPoint: [{ required: true, message: '请选择小数点符号！', trigger: 'blur', },],
	active: [{ required: true, message: '请选择是否启用！', trigger: 'blur', },],
});

// 页面加载时
onMounted(async () => {
});

// 打开弹窗
const openDialog = async (row: any, title: string) => {
	state.title = title;
	row = row ?? { direction: 1, weekStart: 7, active: false };
	state.ruleForm = row.id ? await getAPI(SysLangApi).apiSysLangDetailGet(row.id).then(res => res.data.result) : JSON.parse(JSON.stringify(row));
	state.showDialog = true;
};

// 关闭弹窗
const closeDialog = () => {
	emit("reloadTable");
	state.showDialog = false;
};

// 提交
const submit = async () => {
	ruleFormRef.value.validate(async (isValid: boolean, fields?: any) => {
		if (isValid) {
			if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
				await getAPI(SysLangApi).apiSysLangUpdatePost(state.ruleForm);
			} else {
				await getAPI(SysLangApi).apiSysLangAddPost(state.ruleForm);
			}
			closeDialog();
		} else {
			ElMessage({
				message: `表单有${Object.keys(fields).length}处验证失败，请修改后再提交`,
				type: "error",
			});
		}
	});
};

//将属性或者函数暴露给父组件
defineExpose({ openDialog });
</script>
<template>
	<div class="sysLang-container">
		<el-dialog v-model="state.showDialog" :width="800" draggable :close-on-click-modal="false">
			<template #header>
				<div style="color: #fff">
					<span>{{ state.title }}</span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto" :rules="rules">
				<el-row :gutter="35">
					<el-form-item v-show="false">
						<el-input v-model="state.ruleForm.id" />
					</el-form-item>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="语言名称" prop="name">
							<el-input v-model="state.ruleForm.name" placeholder="请输入语言名称" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="语言代码" prop="code">
							<el-input v-model="state.ruleForm.code" placeholder="请输入语言代码" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="ISO 语言代码" prop="isoCode">
							<el-input v-model="state.ruleForm.isoCode" placeholder="请输入ISO 语言代码" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="URL 语言代码" prop="urlCode">
							<el-input v-model="state.ruleForm.urlCode" placeholder="请输入URL 语言代码" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="书写方向" prop="direction">
							<g-sys-dict v-model="state.ruleForm.direction" code="DirectionEnum" render-as="select"
								placeholder="请选书写方向" clearable filterable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="日期格式" prop="dateFormat">
							<el-input v-model="state.ruleForm.dateFormat" placeholder="请输入日期格式" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="时间格式" prop="timeFormat">
							<el-input v-model="state.ruleForm.timeFormat" placeholder="请输入时间格式" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="每周起始日" prop="weekStart">
							<g-sys-dict v-model="state.ruleForm.weekStart" code="WeekEnum" render-as="select"
								placeholder="请选每周起始日" clearable filterable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="分组符号" prop="grouping">
							<el-input v-model="state.ruleForm.grouping" placeholder="请输入分组符号" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="小数点符号" prop="decimalPoint">
							<el-input v-model="state.ruleForm.decimalPoint" placeholder="请输入小数点符号" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="千分位分隔符" prop="thousandsSep">
							<el-input v-model="state.ruleForm.thousandsSep" placeholder="请输入千分位分隔符" maxlength="255"
								show-word-limit clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="是否启用" prop="active">
							<el-switch v-model="state.ruleForm.active" active-text="是" inactive-text="否" />
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="() => state.showDialog = false">取 消</el-button>
					<el-button @click="submit" type="primary" v-reclick="1000">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>
<style lang="scss" scoped>
:deep(.el-select),
:deep(.el-input-number) {
	width: 100%;
}
</style>