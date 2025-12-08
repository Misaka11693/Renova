<template>
    <div v-if="isShowBreadcrumb" class="bar-box">
        <SvgIcon :size="16" class="bar-expand-icon" 
            :name="themeConfig.isCollapse ? 'ele-Expand' : 'ele-Fold'" 
            @click="onThemeConfigChange" 
        />

        <el-breadcrumb>
            <transition-group name="breadcrumb">
                <el-breadcrumb-item v-for="(v, k) in state.breadcrumbList" :key="v.path">
                    <div v-if="k === state.breadcrumbList.length - 1" class="bar-breadcrumb-item bar-breadcrumb-item-last">
                        <SvgIcon v-if="themeConfig.isBreadcrumbIcon" :name="v.meta.icon" />
                        <span v-if="v.meta.title">{{ v.meta.title }}</span>
                        <span v-else>{{ v.meta.tagsViewName }}</span>
                    </div>
                    <div v-else class="bar-breadcrumb-item">
                        <a @click.prevent="onBreadcrumbClick(v)"> 
                            <SvgIcon :name="v.meta.icon" v-if="themeConfig.isBreadcrumbIcon" />
                            <span>{{ v.meta.title }}</span>
                        </a>
                    </div>
                </el-breadcrumb-item>
            </transition-group>
        </el-breadcrumb>
    </div>
</template>

<script setup lang="ts" name="layoutBreadcrumb">
import { reactive, computed, onMounted } from 'vue';
import { onBeforeRouteUpdate, useRoute, useRouter, RouteLocationNormalized } from 'vue-router';
import { Local } from '/@/utils/storage';
import other from '/@/utils/other';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useRoutesList } from '/@/stores/routesList';

// 定义变量内容
const stores = useRoutesList();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { routesList } = storeToRefs(stores);
const route = useRoute();
const router = useRouter();
const state = reactive<BreadcrumbState>({
	breadcrumbList: [],
	routeSplit: [],
	routeSplitFirst: '',
	routeSplitIndex: 1,
});

// 动态设置经典、横向布局不显示
const isShowBreadcrumb = computed(() => {
	initRouteSplit(route);
	const { layout, isBreadcrumb } = themeConfig.value;
	if (layout === 'classic' || layout === 'transverse') return false;
	else return isBreadcrumb ? true : false;
});
// 面包屑点击时
const onBreadcrumbClick = (v: RouteItem) => {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars, no-unused-vars
	const { redirect, path } = v;
	if (redirect) router.push(redirect);
	// 如果没有指定重定向，则不跳转
	// else if (path) router.push(path);
};
// 展开/收起左侧菜单点击
const onThemeConfigChange = () => {
	themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
	setLocalThemeConfig();
};
// 存储布局配置
const setLocalThemeConfig = () => {
	Local.remove('themeConfig');
	Local.set('themeConfig', themeConfig.value);
};
// 处理面包屑数据
const getBreadcrumbList = (arr: RouteItems) => {
	arr.forEach((item: RouteItem) => {
		state.routeSplit.forEach((v: string, k: number, arrs: string[]) => {
			if (state.routeSplitFirst === item.path) {
				state.routeSplitFirst += `/${arrs[state.routeSplitIndex]}`;
				!state.breadcrumbList.find((a) => a.path === item.path) && state.breadcrumbList.push(item);
				state.routeSplitIndex++;
				if (item.children) getBreadcrumbList(item.children);
			}
		});
	});
};
// 当前路由字符串切割成数组，并删除第一项空内容
const initRouteSplit = (toRoute: RouteLocationNormalized) => {
	if (!themeConfig.value.isBreadcrumb) return false;
	state.breadcrumbList = [];
	state.routeSplit = toRoute.path.split('/');
	state.routeSplit.shift();
	state.routeSplitFirst = `/${state.routeSplit[0]}`;
	state.routeSplitIndex = 1;
	getBreadcrumbList(routesList.value);
	if (toRoute.name === 'home' || (toRoute.name === 'notFound' && state.breadcrumbList.length > 1)) state.breadcrumbList.shift();
	if (state.breadcrumbList.length > 0) state.breadcrumbList[state.breadcrumbList.length - 1].meta.tagsViewName = other.setTagsViewNameI18n(<RouteToFrom>route);
};
// 页面加载时
onMounted(() => {
	initRouteSplit(route);
});
// 路由更新时
onBeforeRouteUpdate((to) => {
	initRouteSplit(to);
});
</script>

<style lang="scss" scoped>
.bar-box {
    display: inline-flex;
    align-items: center;
    height: 100%;
}

.bar-expand-icon {
    cursor: pointer;
	font-size: 18px;
	color: var(--next-bg-topBarColor);
	height: 100%;
	width: 40px;
	opacity: 0.8;

	&:hover {
		opacity: 1;
	}
}
.bar-breadcrumb-item {
    height: 16px;
    line-height: 16px;
	display: flex;
    align-items: center;

    a { display: inherit; align-items: inherit; font-weight: unset; }
    i { margin-right: 5px; }

    &-last { opacity: 0.7; }
}
</style>
