<template>
	<div class="sys-config-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="900px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef">
				<el-tabs v-model="state.selectedTabName">
					<el-tab-pane label="基础信息" name="1" >
						<el-row :gutter="35">
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="名称" prop="name" :rules="[{ required: true, message: '名称不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.name" placeholder="名称" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="编码" prop="code" :rules="[{ required: true, message: '编码不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.code" placeholder="编码" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="分组" prop="groupName" :rules="[{ required: true, message: '分组不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.groupName" placeholder="分组" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="类型" prop="type" :rules="[{ required: true, message: '类型不能为空', trigger: 'blur' }]">
									<g-sys-dict v-model="state.ruleForm.type" code="TemplateTypeEnum" render-as="select"/>
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
					</el-tab-pane>
					<el-tab-pane label="模板内容" name="2">
						<el-row :gutter="5">
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="内容类型">
									<el-radio-group v-model="state.contentType">
										<el-radio :value="1">富文本</el-radio>
										<el-radio :value="2">纯文本</el-radio>
									</el-radio-group>
									<el-button class="ml35" @click="() => editorRef?.ref?.insertNode({ text: '@(name)' })">插入参数</el-button>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="内容" prop="content" :rules="[{ required: true, message: '内容不能为空', trigger: 'blur' }]" label-position="top">
									<Editor v-model:get-html="state.ruleForm.content" ref="editorRef" height="200px" v-if="state.contentType == 1" />
									<el-input v-model="state.ruleForm.content" v-else type="textarea" rows="15" show-word-limit clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20" style="user-select: none;">
								<el-row :gutter="5">
									<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20" title="双击删除参数项">
										<el-form-item label="预览参数" label-position="top">
											<el-button icon="ele-Plus" text @click="() => state.renderData.push([])"></el-button>
										</el-form-item>
									</el-col>
									<el-col v-for="(item, index) in state.renderData" :key="index" :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb5" @dblclick="() => state.renderData.splice(index, 1)">
										<el-row :gutter="5">
											<el-col :span="8">
												<el-input v-model="item[0]" :placeholder="'参数名' + (index + 1)"/>
											</el-col>
											<el-col :span="16">
												<el-input v-model="item[1]" :placeholder="'参数值' + (index + 1)"/>
											</el-col>
										</el-row>
									</el-col>
								</el-row>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="预览结果：" label-width="85" />
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<span v-html="state.result"></span>
							</el-col>
						</el-row>
					</el-tab-pane>
				</el-tabs>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button @click="showPreView" v-reclick="3000">预览</el-button>
					<el-button type="primary" @click="submit" v-reclick="2000">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditConfig">
import {reactive, ref, watch} from 'vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysTemplateApi } from '/@/api-services/api';
import { UpdateTemplateInput } from '/@/api-services/models';
import Editor from "/@/components/editor/index.vue";
import GSysDict from "/@/components/sysDict/sysDict.vue";

const props = defineProps({
	title: String,
});
const editorRef = ref();
const ruleFormRef = ref();
const emits = defineEmits(['updateData']);
const state = reactive({
	isShowDialog: false,
	selectedTabName: "1",
	renderData: [] as any,
	result: '' as any,
	contentType: 1,
	ruleForm: {} as UpdateTemplateInput,
});

const getRenderData = () => {
	const data = {} as any;
	state.renderData.forEach((e: [string, string]) => data[e[0]] = e[1]);
	return data;
}

const getRenderContent = async () => {
	const res = await getAPI(SysTemplateApi).apiSysTemplateRenderPost({ content: state.ruleForm.content, data: getRenderData() })
	state.result = res.data.result;
}

const showPreView = () => {
	getRenderContent();
	state.selectedTabName = '2';
}

// 打开弹窗
const openDialog = (row: any) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.selectedTabName = "1";
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('updateData');
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
			await getAPI(SysTemplateApi).apiSysTemplateUpdatePost(state.ruleForm);
		} else {
			await getAPI(SysTemplateApi).apiSysTemplateAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

watch(
		() => state.ruleForm.content,
		() => {
			state.ruleForm.content?.match(/@\((.*?)\)/g)?.forEach((pa: string, index) => {
				const key = pa.substring(2, pa.length - 1);
				if (!state.renderData.find((e: [string, string]) => e[0] === key)) {
					state.renderData.push([key, '参数' + (index + 1)]);
				}
			});
		}
)

// 导出对象
defineExpose({ openDialog });
</script>
