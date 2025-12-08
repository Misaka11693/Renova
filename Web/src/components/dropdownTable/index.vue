<template>
	<el-select
			v-model="state.selectedValues"
			:clearable="clearable"
			:multiple="multiple"
			:disabled="disabled"
			:placeholder="placeholder"
			:allow-create="allowCreate"
			:remote-method="remoteMethod"
			:default-first-option="allowCreate"
			@visible-change="remoteMethod()"
			remote-show-suffix
			filterable
			ref="selectRef"
			remote>
		<el-option style="display:none" />
		<el-option v-for="item in state.tableData.items" :key="item[valueField]" :label="item[displayField]" :value="item[valueField]" style="display:none" />
		<el-table :data="state.tableData.items" @row-click="handleChange" :height="props.dropdownHeight" v-loading="state.loading" ref="tableRef" :style="{ width: props.dropdownWidth }" highlight-current-row >
			<template #empty><el-empty :image-size="25" /></template>
			<slot name="columns"></slot>
		</el-table>
	</el-select>
</template>

<script lang="ts" setup>
import { reactive, ref, watch } from 'vue';
import {debounce} from "lodash-es";

const props = defineProps({
	modelValue: [String, Number, null],
	fetchOptions: {
		type: Function,
		required: true,
	},
	valueField: {
		type: String,
		default: 'id'
	},
	displayField: {
		type: String,
		default: 'name'
	},
	keywordField: {
		type: String,
		default: 'keyword'
	},
	dropdownWidth: {
		type: String,
		default: '100%'
	},
	dropdownHeight: {
		type: String,
		default: '300px'
	},
	placeholder: {
		type: String,
		default: '请输入关键词'
	},
	queryParams: {
		type: Object,
		default: () => {
			return {
				page: 1,
				pageSize: 20
			}
		}
	},
	allowCreate: Boolean,
	disabled: Boolean,
	multiple: Boolean,
	clearable: Boolean,
});

const tableRef = ref();
const selectRef = ref();
const emit = defineEmits(['update:modelValue', 'change'])
const state = reactive({
	selectedValues: '' as string | string[],
	tableQuery: {
		[props.keywordField]: '',
		page: 1,
		pageSize: props.queryParams?.pageSize ?? 100
	},
	tableData: {
		items: [],
		total: 0
	},
	loading: false
})

// 远程查询方法
const remoteMethod = debounce((query: string = '') => {
	if (query) {
		state.loading = true;
		state.tableQuery[props.keywordField] = query?.trim();
		props.fetchOptions(Object.assign({}, props.queryParams, state.tableQuery)).then((result: any) => {
			state.tableData.items = result.items;
			state.tableData.total = result.total;
			state.loading = false;
		});
	} else {
		state.tableData.items = [];
		state.tableData.total = 0;
	}
}, 500);

// 选择值改变事件
const handleChange = (row: any) => {
	if (props.multiple && !state.selectedValues) state.selectedValues = [];
	if (typeof row[props.valueField] === 'string') row[props.valueField] = row[props.valueField]?.trim();
	state.selectedValues = props.multiple ? Array.from(new Set([...state.selectedValues, row[props.valueField]])) : row[props.valueField];
	emit('update:modelValue', state.selectedValues);
	emit('change', state.selectedValues, row);
	if (!props.multiple) selectRef.value?.blur();
	tableRef.value?.setCurrentRow(row);
};

watch(
		() => props.modelValue,
		(val: any) => { state.selectedValues = val }
);
</script>

<style scoped>
:deep(.el-select-dropdown) {
	.el-scrollbar > .el-scrollbar__bar {
		display: none!important;
	}
}
</style>