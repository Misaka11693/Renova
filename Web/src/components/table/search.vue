<template>
	<div style="display: flex;">
        <!-- <div class="table-search-container"> -->
        <div :class="['table-search-container', { 'table-search-flex': search.length > defaultShowCount }]">
            <el-form ref="tableSearchRef" :model="state.innerModelValue" :inline="true" label-width="100px">
                <span v-for="(val, key) in search" :key="key" v-show="key < defaultShowCount || state.isToggle">
                    <template v-if="val.type">
                        <el-form-item
                                label-width="auto"
                                :label="val.label"
                                :prop="val.prop"
                                :rules="[{ required: val.required, message: `${val.label}不能为空`, trigger: val.type === 'input' ? 'blur' : 'change' }]"
                            >
                                <el-input
                                    v-model="state.innerModelValue[val.prop]"
                                    v-bind="val.comProps"
                                    :placeholder="val.placeholder"
                                    :clearable="!val.required"
                                    v-if="val.type === 'input'"
                                    @keyup.enter="onSearch(tableSearchRef)"
                                    @change="val.change"
                                />
                                <el-date-picker
                                    v-model="state.innerModelValue[val.prop]"
                                    v-bind="val.comProps"
                                    type="date"
                                    :placeholder="val.placeholder"
                                    :clearable="!val.required"
                                    v-else-if="val.type === 'date'"
                                    @change="val.change"
                                />
                                <el-date-picker
                                    v-model="state.innerModelValue[val.prop]"
                                    v-bind="val.comProps"
                                    type="monthrange"
                                    value-format="YYYY/MM/DD"
                                    :placeholder="val.placeholder"
                                    :clearable="!val.required"
                                    v-else-if="val.type === 'monthrange'"
                                    @change="val.change"
                                />
                                <el-date-picker
                                    v-model="state.innerModelValue[val.prop]"
                                    v-bind="val.comProps"
                                    type="daterange"
                                    value-format="YYYY/MM/DD"
                                    range-separator="至"
                                    start-placeholder="开始日期"
                                    end-placeholder="结束日期"
                                    :clearable="!val.required"
                                    :shortcuts="shortcuts"
                                    :default-time="defaultTime"
                                    v-else-if="val.type === 'daterange'"
                                    @change="val.change"
                                />
                                <el-select
                                    v-model="state.innerModelValue[val.prop]"
                                    v-bind="val.comProps"
                                    :clearable="!val.required"
                                    :placeholder="val.placeholder"
                                    v-else-if="val.type === 'select'"
                                    @change="val.change"
                                >
                                    <el-option v-for="item in getSelectOptions(val)" :key="item.value" :label="item.label" :value="item.value" />
                                </el-select>
                                <el-cascader
                                    v-else-if="val.type === 'cascader' && val.cascaderData"
                                    :options="val.cascaderData"
                                    :clearable="!val.required"
                                    filterable
                                    :props="val.cascaderProps ? val.cascaderProps : state.cascaderProps"
                                    :placeholder="val.placeholder"
                                    @change="val.change"
                                    v-bind="val.comProps"
                                    v-model="state.innerModelValue[val.prop]"
                                >
                                </el-cascader>
                        </el-form-item>
                    </template>
                </span>
            </el-form>
        </div>
        <div v-if="search.length > defaultShowCount" class="table-search-more">
            <el-form :inline="true">
                <el-form-item>
                    <el-button text @click="state.isToggle = !state.isToggle"
                    >更多查询
                        <el-icon class="el-icon--right">
                            <ele-ArrowUpBold v-if="state.isToggle" />
                            <ele-ArrowDownBold v-else />
                        </el-icon>
                </el-button>
                    
                    
                </el-form-item>
            </el-form>
        </div>
        <div class="table-search-btn">
            <el-form :inline="true">
                <el-form-item>
                    <!-- 使用el-button-group会导致具有type属性的按钮的右边框无法显示 -->
                    <!-- <el-button-group> -->
                    <el-button plain type="primary" icon="ele-Search" @click="onSearch(tableSearchRef)"> 查询 </el-button>
                    <el-button icon="ele-Refresh" @click="onReset(tableSearchRef)" style="margin-left: 12px"> 重置 </el-button>
                    <!-- </el-button-group> -->
                </el-form-item>
            </el-form>
        </div>
    </div>
</template>

<script setup lang="ts" name="makeTableDemoSearch">
import { reactive, ref, watch } from 'vue';
import type { FormInstance } from 'element-plus';
import { dayjs } from 'element-plus';
import {useUserInfo} from "/@/stores/userInfo";

// 定义父组件传过来的值
const props = defineProps({
	// 搜索表单,type-控件类型（input,select,cascader,date）,options-type为selct时需传值，cascaderData,cascaderProps-type为cascader时需传值，属性同elementUI,cascaderProps不传则使用state默认。
	// 可带入comProps属性，和使用的控件属性对应
	search: {
		type: Array<TableSearchType>,
		default: () => [],
	},
	modelValue: {
		type: Object,
		default: () => ({}),
	},
    // 默认显示几个查询条件，超过则隐藏，点击更多展开
    defaultShowCount: {
        type: Number,
        default: 5,
    },
});

// 定义子组件向父组件传值/事件
const emit = defineEmits(['search', 'reset', 'update:modelValue']);

// 定义变量内容
const tableSearchRef = ref<FormInstance>();
const state = reactive({
	isToggle: false,
	cascaderProps: { checkStrictly: true, emitPath: false, value: 'id', label: 'name', expandTrigger: 'hover' },
	/** 内部 modelValue */
	innerModelValue: {} as EmptyObjectType,
});

/** 监听 props.modelValue 变化 */
watch(
	() => props.modelValue,
	(val) => {
		state.innerModelValue = val;
	},
	{ immediate: true }
);

/** 监听 state.innerModelValue 变化 */
watch(
	() => state.innerModelValue,
	(val) => {
		emit('update:modelValue', val);
	},
	{ deep: true }
);

// 查询
const onSearch = (formEl: FormInstance | undefined) => {
	if (!formEl) return;
	formEl.validate((isValid: boolean): void => {
		if (!isValid) return;

		emit('search', state.innerModelValue);
	});
};

// 重置
const onReset = (formEl: FormInstance | undefined) => {
	if (!formEl) return;
	formEl.resetFields();
	emit('reset', state.innerModelValue);
};

const userStore = useUserInfo();
const getSelectOptions = (val: TableSearchType) => {
	if (val.options) return val.options;
	if (val.dictCode) return userStore.getDictDataByCode(val.dictCode);
	return [];
};

/** 时间范围默认时间 */
const defaultTime = ref<[Date, Date]>([new Date(2000, 1, 1, 0, 0, 0), new Date(2000, 2, 1, 23, 59, 59)]);
/** 时间范围快捷选择 */
const shortcuts = [
	{
		text: '7天内',
		value: () => {
			const end = dayjs().endOf('day').toDate();
			const start = dayjs().startOf('day').add(-7, 'day').toDate();
			return [start, end];
		},
	},
	{
		text: '1个月内',
		value: () => {
			const end = dayjs().endOf('day').toDate();
			const start = dayjs().startOf('day').add(-1, 'month').toDate();
			return [start, end];
		},
	},
	{
		text: '3个月内',
		value: () => {
			const end = dayjs().endOf('day').toDate();
			const start = dayjs().startOf('day').add(-3, 'month').toDate();
			return [start, end];
		},
	},
];
</script>

<style scoped lang="scss">
.table-search-flex {
    flex: 1;
}

.table-search-container {
    //flex: 1;

    :deep(.el-form-item--small .el-form-item__label) {
        padding: 0 8px 0 0;
    }
}

.table-search-more {
	border-right: 1px solid var(--el-card-border-color);
}

.table-search-btn {
    flex-shrink: 0;

    // 右侧查询重置按钮随展开垂直居中
    // .el-form--inline {
    //     height: 100%;
	// 	.el-form-item--small.el-form-item,.el-form-item:last-of-type {
    //         height: calc(100% - 10px);
    //     }
	// }
}
</style>
