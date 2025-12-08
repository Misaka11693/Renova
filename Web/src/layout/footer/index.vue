<template>
	<div class="layout-footer">
		<div class="layout-footer-warp">
			<!-- <div>{{ themeConfig.globalTitle }}</div> -->
			<div>{{ themeConfig.copyright }} {{ themeConfig.globalTitle }}</div>
		</div>
	</div>
</template>

<script setup lang="ts" name="layoutFooter">
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
</script>

<style scoped lang="scss">
.layout-footer {
	width: 100%;
	display: flex;
    line-height: 30px;
    height: 30px;
    background: var(--el-bg-color);
    border-top: 1px solid var(--el-border-color-light);

	&-warp {
		margin: auto;
		color: var(--el-text-color-secondary);
		text-align: center;
		animation: error-num 0.3s ease;
	}
}
</style>
