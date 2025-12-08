<template>
	<el-card class="box-card" shadow="hover" style="height: 100%" body-style="height:100%; overflow:auto">
		<template #header>
			<div class="card-header">
				<div class="tree-h-flex" v-if="!props.tenantId">
					<el-select v-if="userStore.userInfos.accountType == 999" v-model="state.tenantId" @change="initTreeData()" placeholder="请选择租户" class="w100 mb10">
						<el-option :value="item.value" :label="`${item.label} (${item.host})`" v-for="(item, index) in state.tenantList" :key="index" />
					</el-select>
				</div>
				<div class="tree-h-flex">
					<div class="tree-h-left">
						<el-input :prefix-icon="Search" v-model="filterText" placeholder="机构名称" />
					</div>
					<div class="tree-h-right">
						<el-dropdown @command="handleCommand">
							<el-button style="margin-left: 8px; width: 34px">
								<el-icon class="el-icon--center">
									<more-filled />
								</el-icon>
							</el-button>
							<template #dropdown>
								<el-dropdown-menu>
									<el-dropdown-item command="expandAll">全部展开</el-dropdown-item>
									<el-dropdown-item command="collapseAll">全部折叠</el-dropdown-item>
									<el-dropdown-item command="rootNode">根节点</el-dropdown-item>
									<el-dropdown-item command="refresh">刷新</el-dropdown-item>
								</el-dropdown-menu>
							</template>
						</el-dropdown>
					</div>
				</div>
			</div>
		</template>
		<div style="margin-bottom: 45px" v-loading="state.loading">
			<el-tree
				ref="treeRef"
				class="filter-tree"
				:data="state.orgData"
				node-key="id"
				:props="{ children: 'children', label: 'name' }"
				:filter-node-method="filterNode"
				@node-click="nodeClick"
				:show-checkbox="state.isShowCheckbox"
				:default-checked-keys="state.ownOrgData"
				highlight-current
				check-strictly
			>
				<template #default="{ node }">
					<el-icon v-if="node.level == 1" size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-School /></el-icon>
					<el-icon v-else-if="node.level == 2" size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-PriceTag /></el-icon>
					<el-icon v-else size="16" style="margin-right: 3px; display: inline; vertical-align: middle"><ele-CollectionTag /></el-icon>
					{{ node.label }}
				</template>
			</el-tree>
		</div>
	</el-card>
</template>

<script lang="ts" setup>
import { onMounted, reactive, ref, watch } from 'vue';
import type { ElTree } from 'element-plus';
import { Search, MoreFilled } from '@element-plus/icons-vue';

import { getAPI } from '/@/utils/axios-utils';
import {SysOrgApi, SysTenantApi} from '/@/api-services/api';
import { OrgTreeOutput, SysOrg } from '/@/api-services/models';
import { useUserInfo } from "/@/stores/userInfo";

const props = defineProps({
	tenantId: Number,
});
const userStore = useUserInfo();
const filterText = ref('');
const treeRef = ref<InstanceType<typeof ElTree>>();
const state = reactive({
	loading: false,
	tenantList: [] as Array<any>,
	tenantId: props.tenantId as number,
	orgData: [] as Array<OrgTreeOutput>,
	isShowCheckbox: false,
	ownOrgData: [] as Array<OrgTreeOutput>,
});

onMounted( async () => {
	if (userStore.userInfos.accountType == 999) {
		state.tenantList = await getAPI(SysTenantApi).apiSysTenantListGet().then(res => res.data.result ?? []);
	}
	initTreeData();
});

watch(filterText, (val) => {
	treeRef.value!.filter(val);
});

const initTreeData = async () => {
	state.loading = true;
	const res = await getAPI(SysOrgApi).apiSysOrgTreeGet(0, undefined, undefined, undefined, state.tenantId);
	state.orgData = res.data.result ?? [];
	state.loading = false;
};

// 设置默认选择
const setCheckedKeys = (data: any) => {
	const isArray = Array.isArray(data);
	treeRef.value!.setCheckedKeys([]);
	if (!isArray) {
		treeRef.value!.setCurrentNode(data);
		nodeClick(data);
	}
	state.ownOrgData = isArray ? data : [data];
	state.isShowCheckbox = isArray;
};

// 获取已经选择
const getCheckedKeys = () => {
	return treeRef.value!.getCheckedKeys();
};

const filterNode = (value: string, data: any) => {
	if (!value) return true;
	return data.name.includes(value);
};

const handleCommand = async (command: string | number | object) => {
	if ('expandAll' == command) {
		for (let i = 0; i < treeRef.value!.store._getAllNodes().length; i++) {
			treeRef.value!.store._getAllNodes()[i].expanded = true;
		}
	} else if ('collapseAll' == command) {
		for (let i = 0; i < treeRef.value!.store._getAllNodes().length; i++) {
			treeRef.value!.store._getAllNodes()[i].expanded = false;
		}
	} else if ('refresh' == command) {
		initTreeData();
	} else if ('rootNode' == command) {
		emits('node-click', { id: 0, name: '' });
	}
};

// 与父组件的交互逻辑
const emits = defineEmits(['node-click']);
const nodeClick = (node: any) => {
	emits('node-click', node);
};

// 导出对象
defineExpose({ initTreeData, setCheckedKeys, getCheckedKeys });
</script>

<style lang="scss" scoped>
.tree-h-flex {
	display: flex;
}

.tree-h-left {
	flex: 1;
	width: 100%;
}

.tree-h-right {
	width: 42px;
	min-width: 42px;
}
</style>
