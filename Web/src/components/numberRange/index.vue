<!-- 
// NumberRange组件，用于输入数字范围(支持前缀和后缀插槽，支持数字精度，支持限制取值范围)，在数值、金额等的范围输入场景中使用
// 使用示例：
<template>
	<el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
		<el-form-item label="订单金额">
			<number-range v-model="queryParams.amountRange">
				<template #prepend>
					<span>范围</span>
				</template>
			</number-range>
		</el-form-item>
	</el-col>
</template>
<script lang="ts" setup>
import NumberRange from '/@/components/numberRange/index.vue';
</script>
-->

<template>
	<div class="number-range-container">
		<div :id="usePrepend ? 'prepend' : ''" :class="{ 'slot-default': slotStyle === 'default', 'slot-pend ': usePrepend }">
			<slot name="prepend">
				<!-- 前缀插槽 -->
			</slot>
		</div>
		<div
			class="number-range"
			:class="{
				'is-disabled': disabled,
				'is-focus': isFocus,
				'number-range-left-border-radius-0': usePrepend,
				'number-range-right-border-radius-0': useAppend,
			}"
		>
			<el-input-number
				:disabled="disabled"
				placeholder="最小值"
				@blur="handleBlur"
				@focus="handleFocus"
				@change="handleChangeMinValue"
				@update:modelValue="updateMinValue"
				v-model="minValue_"
				v-bind="$attrs"
				:controls="false"
			/>
			<div class="to">
				<span>{{ to }}</span>
			</div>
			<el-input-number
				:disabled="disabled"
				placeholder="最大值"
				@blur="handleBlur"
				@focus="handleFocus"
				@change="handleChangeMaxValue"
				@update:modelValue="updateMaxValue"
				v-model="maxValue_"
				v-bind="$attrs"
				:controls="false"
			/>
			<!-- 清除图标 -->
			<el-icon v-if="clearable && (minValue_ || maxValue_)" class="el-icon el-input__icon el-input__clear" @click="clearValues">
				<CircleClose />
			</el-icon>
		</div>
		<div :id="useAppend ? 'append' : ''" :class="{ 'slot-default': slotStyle === 'default', 'slot-pend ': useAppend }">
			<slot name="append">
				<!-- 后缀插槽 -->
			</slot>
		</div>
	</div>
</template>
<script lang="ts" setup name="numberRange">
import { computed, ref, useSlots } from 'vue';
import { CircleClose } from '@element-plus/icons-vue';

const props = defineProps({
	modelValue: {
		type: Array<Number>,
		default: () => [null, null], // 调用时使用v-model="[min,max]" 绑定
	},
	clearable: {
		type: Boolean,
		default: false,
	},
	minValue: {
		type: Number,
		default: null, // 调用时使用v-model:min-value="" 绑定多个v-model
	},
	maxValue: {
		type: Number,
		default: null, // 调用时使用v-model:max-value="" 绑定多个v-model
	},
	// 是否禁用
	disabled: {
		type: Boolean,
		default: false,
	},
	to: {
		type: String,
		default: '-',
	},
	// 精度参数 -保留小数位数
	precision: {
		type: Number,
		default: 0,
		validator(val: number) {
			return val >= 0 && val === parseInt(String(val), 10);
		},
	},
	// 限制取值范围
	valueRange: {
		type: Array,
		default: () => [],
		validator(val: []) {
			if (val && val.length > 0) {
				// @ts-ignore
				if (val.length !== 2) {
					throw new Error('请传入长度为2的Number数组');
				}
				// @ts-ignore
				if (typeof val[0] !== 'number' || typeof val[1] !== 'number') {
					throw new Error('取值范围只接受Number类型,请确认');
				}
				// @ts-ignore
				if (val[1] < val[0]) {
					throw new Error('valueRange格式须为[最小值,最大值],请确认');
				}
			}
			return true;
		},
	},
	// 插槽样式
	slotStyle: {
		type: String, // default --异色背景 |  plain--无背景色
		default: 'plain',
	},
});

const emit = defineEmits(['update:modelValue', 'update:minValue', 'update:maxValue', 'change']);

const minValue_ = computed({
	get() {
		return props.minValue || props.modelValue[0] || null;
	},
	set(value) {
		if (value === null) {
			emit('update:minValue', null);
			emit('update:modelValue', [null, maxValue_.value]);
			return;
		}
		emit('update:minValue', value);
		emit('update:modelValue', [value, maxValue_.value]);
	},
});

const maxValue_ = computed({
	get() {
		return props.maxValue || props.modelValue[1] || null;
	},
	set(value) {
		if (value === null) {
			emit('update:maxValue', null);
			emit('update:modelValue', [minValue_.value, null]);
			return;
		}
		emit('update:maxValue', value);
		emit('update:modelValue', [minValue_.value, value]);
	},
});

// 清除值的方法
const clearValues = () => {
	minValue_.value = null;
	maxValue_.value = null;
	emit('update:modelValue', [null, null]);
};

const handleChangeMinValue = (value: number | null) => {
	// 非数字空返回null
	if (value === null || isNaN(value)) {
		emit('update:minValue', null);
		return;
	}
	// 初始化数字精度
	const newMinValue = parsePrecision(value, props.precision);
	// min > max 交换min max
	if (typeof newMinValue === 'number' && parseFloat(String(newMinValue)) > parseFloat(String(maxValue_.value))) {
		// 取值范围判定
		const { min, max } = decideValueRange(Number(maxValue_.value), newMinValue);
		// 更新绑定值
		updateValue(min, max);
	}
};

const handleChangeMaxValue = (value: number | null) => {
	// 非数字空返回null
	if (value === null || isNaN(value)) {
		emit('update:maxValue', null);
		return;
	}
	// 初始化数字精度
	const newMaxValue = parsePrecision(value, props.precision);
	// max < min 交换min max
	if (typeof newMaxValue === 'number' && parseFloat(String(newMaxValue)) < parseFloat(String(minValue_.value))) {
		// 取值范围判定
		const { min, max } = decideValueRange(newMaxValue, Number(minValue_.value));
		// 更新绑定值
		updateValue(min, max);
	}
};

const updateMinValue = (value: number | null) => {
	minValue_.value = value;
};

const updateMaxValue = (value: number | null) => {
	maxValue_.value = value;
};

// 更新数据
const updateValue = (min: number | null, max: number | null) => {
	emit('update:minValue', min);
	emit('update:maxValue', max);
	emit('update:modelValue', [min, max]);
	emit('change', { min, max });
};

// 取值范围判定
const decideValueRange = (min: number | null, max: number | null) => {
	if (min === null || max === null) {
		return { min, max };
	}

	if (props.valueRange && props.valueRange.length > 0) {
		// @ts-ignore
		min = min < props.valueRange[0] ? props.valueRange[0] : min > props.valueRange[1] ? props.valueRange[1] : min;
		// @ts-ignore
		max = max > props.valueRange[1] ? props.valueRange[1] : max;
	}
	return { min, max };
};

// input焦点事件
const isFocus = ref();

const handleFocus = () => {
	isFocus.value = true;
};

const handleBlur = () => {
	isFocus.value = false;
};

// 处理数字精度
const parsePrecision = (number: number | null, precision = 0) => {
	if (number === null) {
		return null;
	}
	return parseFloat(String(Math.round(number * Math.pow(10, precision)) / Math.pow(10, precision)));
};

// 判断插槽是否被使用
// 组件外部使用时插入了
// <template #插槽名 >
// </template>
// 无论template标签内是否插入了内容，均视为已使用该插槽
const slots = useSlots();
const usePrepend = computed(() => {
	// 前缀插槽
	return slots && slots.prepend ? true : false;
});
const useAppend = computed(() => {
	// 后缀插槽
	return slots && slots.append ? true : false;
});
</script>
<style lang="scss" scoped>
.number-range-container {
	position: relative;
	display: flex;
	height: 100%;
	.slot-pend {
		white-space: nowrap;
		color: var(--el-color-info);
		border-radius: var(--el-input-border-radius, var(--el-border-radius-base));
	}
	#prepend {
		padding: 0 20px;
		box-shadow:
			1px 0 0 0 var(--el-input-border-color, var(--el-border-color)) inset,
			0 1px 0 0 var(--el-input-border-color, var(--el-border-color)) inset,
			0 -1px 0 0 var(--el-input-border-color, var(--el-border-color)) inset;
		border-right: 0;
		border-top-right-radius: 0;
		border-bottom-right-radius: 0;
	}
	#append {
		padding: 0 20px;
		box-shadow:
			0 1px 0 0 var(--el-input-border-color, var(--el-border-color)) inset,
			0 -1px 0 0 var(--el-input-border-color, var(--el-border-color)) inset,
			-1px 0 0 0 var(--el-input-border-color, var(--el-border-color)) inset;
		border-left: 0;
		border-top-left-radius: 0;
		border-bottom-left-radius: 0;
	}
	.slot-default {
		background-color: var(--el-fill-color-light);
	}

	.number-range-left-border-radius-0 {
		border-top-left-radius: 0 !important;
		border-bottom-left-radius: 0 !important;
	}
	.number-range-right-border-radius-0 {
		border-top-right-radius: 0 !important;
		border-bottom-right-radius: 0 !important;
	}

	.number-range {
		background-color: var(--el-bg-color) !important;
		box-shadow: 0 0 0 1px var(--el-input-border-color, var(--el-border-color)) inset;
		border-radius: var(--el-input-border-radius, var(--el-border-radius-base));
		padding: 0 2px;
		display: flex;
		flex-direction: row;
		width: 100%;
		justify-content: center;
		align-items: center;
		color: var(--el-input-text-color, var(--el-text-color-regular));
		transition: var(--el-transition-box-shadow);
		transform: translate3d(0, 0, 0);
		overflow: hidden;

		.to {
			margin-top: 1px;
		}
	}

	.is-focus {
		transition: all 0.3s;
		box-shadow: 0 0 0 1px var(--el-color-primary) inset !important;
	}
	.is-disabled {
		background-color: var(--el-input-bg-color);
		color: var(--el-input-text-color, var(--el-text-color-regular));
		cursor: not-allowed;
		.to {
			height: calc(100% - 3px);
			background-color: var(--el-fill-color-light) !important;
		}
	}
}

.el-input__clear {
	cursor: pointer;
	color: var(--el-input-icon-color, var(--el-text-color-placeholder));
	margin-left: 10px;
	display: none;
	position: absolute;
	right: 10px;
	top: 50%;
	transform: translateY(-50%);
}

.number-range:hover .el-input__clear {
	display: flex;
	align-items: center;
	color: var(--el-input-clear-hover-color);
}

:deep(.el-input) {
	border: none;
}
:deep(.el-input__wrapper) {
	margin: 0;
	padding: 0 15px;
	background-color: transparent;
	border: none !important;
	box-shadow: none !important;
	&.is-focus {
		border: none !important;
		box-shadow: none !important;
	}
}

:deep(.el-input),
:deep(.el-select),
:deep(.el-input-number) {
	width: 100%;
}
</style>
