<template>
	<div class="h100">
		<CardPro shadow="hover" v-loading="state.isLoading" style="height: 100%;">
			<el-descriptions title="系统信息配置" :column="2" :border="true">
				<template #title>
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Setting /> </el-icon> 系统信息配置
				</template>
				<el-descriptions-item label="系统图标" :span="2">
					<el-upload ref="uploadRef" class="avatar-uploader" :showFileList="false" :autoUpload="false" accept=".jpg,.png,.svg" action :limit="1" :onChange="handleUploadChange">
						<img v-if="state.formData.logo" :src="state.formData.logo" class="avatar" />
						<SvgIcon v-else class="avatar-uploader-icon" name="ele-Plus" :size="28" />
					</el-upload>
				</el-descriptions-item>
				<el-descriptions-item label="系统主标题">
					<el-input v-model="state.formData.title" />
				</el-descriptions-item>
				<el-descriptions-item label="系统副标题">
					<el-input v-model="state.formData.viceTitle" />
				</el-descriptions-item>
				<el-descriptions-item label="系统描述" :span="2">
					<el-input v-model="state.formData.viceDesc" />
				</el-descriptions-item>
				<el-descriptions-item label="水印内容" :span="2">
					<el-input v-model="state.formData.watermark" placeholder="如果此处留空，则水印功能将被禁用"/>
				</el-descriptions-item>
				<el-descriptions-item label="版权说明" :span="2">
					<el-input v-model="state.formData.copyright" />
				</el-descriptions-item>
				<el-descriptions-item label="ICP备案号">
					<el-input v-model="state.formData.icp" />
				</el-descriptions-item>
				<el-descriptions-item label="ICP地址">
					<el-input v-model="state.formData.icpUrl" />
				</el-descriptions-item>
				<!-- <el-descriptions-item label="图形验证码">
					<g-sys-dict v-model="state.formData.captcha" code="YesNoEnum" render-as="radio" />
				</el-descriptions-item>
				<el-descriptions-item label="登录二次验证">
					<g-sys-dict v-model="state.formData.secondVer" code="YesNoEnum" render-as="radio" />
				</el-descriptions-item> -->
				<el-descriptions-item label="用户注册">
					<g-sys-dict v-model="state.formData.enableReg" code="YesNoEnum" render-as="radio" />
				</el-descriptions-item>
				<el-descriptions-item label="注册方案" v-if="state.formData.enableReg == 1">
					<el-select v-model="state.formData.regWayId" placeholder="注册方案" clearable class="w100">
						<el-option :label="item.label" :value="item.value" v-for="(item, index) in state.wayList" :key="index" />
					</el-select>
				</el-descriptions-item>
				<template #extra>
					<el-button type="primary" icon="ele-SuccessFilled" @click="onSave">保存</el-button>
				</template>
			</el-descriptions>
		</CardPro>
	</div>
</template>

<script setup lang="ts" name="sysInfoSetting">
import { nextTick, reactive, ref } from 'vue';
import { ElMessage, UploadInstance } from 'element-plus';
import { fileToBase64 } from '/@/utils/base64Conver';

import { getAPI } from '/@/utils/axios-utils';
import { SysConfigApi } from '/@/api-services';
import GSysDict from '/@/components/sysDict/sysDict.vue';
import CardPro from '/@/components/CardPro/index.vue';

const uploadRef = ref<UploadInstance>();
const state = reactive({
	isLoading: false,
	file: undefined as any,
	wayList: [] as Array<any>,
	formData: {
		logo: '',
		logoBase64: '',
		logoFileName: '',
		title: '',
		viceTitle: '',
		viceDesc: '',
		watermark: '',
		copyright: '',
		icp: '',
		icpUrl: '',
		regWayId: undefined,
		enableReg: undefined,
		secondVer: undefined,
		captcha: undefined,
	},
});

// 通过onChange方法获得文件列表
const handleUploadChange = (file: any) => {
	uploadRef.value!.clearFiles();
	state.file = file;
	state.formData.logo = URL.createObjectURL(state.file.raw); // 显示预览logo
};

// 保存
const onSave = async () => {
	// 如果有选择图标，则转换为 base64
	if (state.file) {
		state.formData.logoBase64 = (await fileToBase64(state.file.raw)) as string;
		state.formData.logoFileName = state.file.raw.name;
	}

	try {
		state.isLoading = true;
		if (state.formData.enableReg == 2) {
			state.formData.regWayId = undefined;
		} else if (!state.formData.regWayId) {
			ElMessage.error('注册方案不能为空');
			return;
		}
		await getAPI(SysConfigApi).apiSysConfigSaveSysInfoPost(state.formData);

		// 清空 file 变量
		state.file = undefined;
		await loadData();
		ElMessage.success('保存成功');
	} finally {
		nextTick(() => {
			state.isLoading = false;
		});
	}
};

// 加载数据
const loadData = async () => {
	try {
		state.isLoading = true;
		const res = await getAPI(SysConfigApi).apiSysConfigSysInfoGet();
		if (res.data!.type !== 'success') return;

		const result = res.data.result;
		state.wayList = result.wayList ?? [];
		result.wayList = undefined;
		state.formData = res.data.result;
	} finally {
		nextTick(() => {
			state.isLoading = false;
		});
	}
};

loadData();
</script>

<style lang="scss" scoped>
.avatar-uploader .avatar {
	width: 100px;
	height: 100px;
	display: block;
	object-fit: contain;
}

:deep(.avatar-uploader) .el-upload {
	border: 1px dashed var(--el-border-color);
	cursor: pointer;
	position: relative;
	overflow: hidden;
	transition: var(--el-transition-duration-fast);
}

:deep(.avatar-uploader) .el-upload:hover {
	border-color: var(--el-color-primary);
}

.el-icon.avatar-uploader-icon {
	color: #8c939d;
	width: 100px;
	height: 100px;
	text-align: center;
}
</style>
