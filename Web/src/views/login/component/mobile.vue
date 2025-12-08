<template>
	<el-form size="large" class="login-content-form">
		<el-form-item class="login-animation1" v-if="!props.tenantInfo.id && !themeConfig.hideTenantForLogin">
			<el-select v-model="state.ruleForm.tenantId" placeholder="请选择租户" clearable style="width: 100%" filterable>
				<template #prefix>
					<i class="iconfont icon-shuxingtu el-input__icon"></i>
				</template>
				<el-option :value="item.value" :label="item.label" v-for="(item, index) in tenantInfo.list" :key="index" />
			</el-select>
		</el-form-item>
		<el-form-item class="login-animation1">
			<el-input text placeholder="请输入手机号" v-model="state.ruleForm.phone" clearable autocomplete="off">
				<template #prefix>
					<i class="iconfont icon-dianhua el-input__icon"></i>
				</template>
			</el-input>
		</el-form-item>
		<el-form-item class="login-animation2">
			<el-col :span="15">
				<el-input text maxlength="6" placeholder="请输入验证码" v-model="state.ruleForm.code" clearable autocomplete="off">
					<template #prefix>
						<el-icon class="el-input__icon"><ele-Position /></el-icon>
					</template>
				</el-input>
			</el-col>
			<el-col :span="1"></el-col>
			<el-col :span="8">
				<el-button v-waves class="login-content-code" :loading="state.loading" :disabled="state.disabled" @click="getSmsCode">
					{{ state.btnText }}
				</el-button>
			</el-col>
		</el-form-item>
		<el-form-item class="login-animation3">
			<el-button round type="primary" v-waves class="login-content-submit" @click="onSignIn">
				<span>登 录</span>
			</el-button>
		</el-form-item>
		<div class="font12 mt30 login-animation4 login-msg">* 温馨提示：建议使用谷歌、Microsoft Edge，版本 79.0.1072.62 及以上浏览器，360浏览器请使用极速模式</div>
	</el-form>
</template>

<script setup lang="ts" name="loginMobile">
import { reactive } from 'vue';
import { ElMessage } from 'element-plus';
import { verifyPhone } from '/@/utils/toolsValidate';
import { getAPI } from '/@/utils/axios-utils';
import { SysSmsApi, SysAuthApi } from '/@/api-services/api';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';

const props = defineProps({
	tenantInfo: {
		required: true,
		type: Object,
	},
});
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const state = reactive({
	ruleForm: {
		tenantId: props.tenantInfo.id,
		phone: '',
		code: '',
	},
	btnText: '获取验证码',
	loading: false,
	disabled: false,
	timer: null as any,
});

// 获取短信验证码
const getSmsCode = async () => {
	state.ruleForm.code = '';
	if (!verifyPhone(state.ruleForm.phone)) {
		ElMessage.error('请正确输入手机号码！');
		return;
	}

	await getAPI(SysSmsApi).apiSysSmsSendSmsPhoneNumberTemplateIdPost(state.ruleForm.phone, '0');

	// 倒计时期间禁止点击
	state.disabled = true;

	// 清除定时器
	state.timer && clearInterval(state.timer);

	// 开启定时器
	var duration = 60;
	state.timer = setInterval(() => {
		duration--;
		state.btnText = `${duration} 秒后重新获取`;
		if (duration <= 0) {
			state.btnText = '获取验证码';
			state.disabled = false; // 恢复按钮可以点击
			clearInterval(state.timer); // 清除掉定时器
		}
	}, 1000);
};

// 登录
const onSignIn = async () => {
	state.ruleForm.tenantId ??= props.tenantInfo.id ?? props.tenantInfo.list[0]?.value ?? undefined;
	const res = await getAPI(SysAuthApi).apiSysAuthLoginPhonePost(state.ruleForm);
	if (res.data.result?.accessToken == undefined) {
		ElMessage.error('登录失败，请检查账号！');
		return;
	}

	// // 系统登录
	// await accountRef.value?.saveTokenAndInitRoutes(res.data.result?.accessToken);
};
</script>

<style scoped lang="scss">
.login-content-form {
	margin-top: 20px;
	@for $i from 1 through 4 {
		.login-animation#{$i} {
			opacity: 0;
			animation-name: error-num;
			animation-duration: 0.5s;
			animation-fill-mode: forwards;
			animation-delay: calc($i/10) + s;
		}
	}
	.login-content-code {
		width: 100%;
		padding: 0;
	}
	.login-content-submit {
		width: 100%;
		letter-spacing: 2px;
		font-weight: 300;
		margin-top: 15px;
	}
	.login-msg {
		color: var(--el-text-color-placeholder);
	}
}
</style>
