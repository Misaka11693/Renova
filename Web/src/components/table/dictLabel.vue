<template>
	<el-Tag v-if="state.dict" :type="state.dict.tagType ?? 'primary'" :style="state.dict.styleSetting" :class="state.dict.classSetting">{{ state.dict[props.propLabel] ?? props.defaultValue }}</el-Tag>
  <span v-else>{{ props.value }}</span>
</template>

<script lang="ts" setup>
import { useUserInfo } from '/@/stores/userInfo';
import { reactive, onMounted, watch } from 'vue';

const props = defineProps({
	code: String,
	value: null,
	propLabel: {
		type: String,
		default: 'label',
	},
	propValue: {
		type: String,
		default: 'value',
	},
	defaultValue: {
		type: String,
		default: '-',
	},
});

const tagTypeMap = {
  "success": 1,
  "warning": 1,
  "info": 1,
  "primary": 1,
  "danger": 1
} as any;

const state = reactive({
  dict: null as any,
});

onMounted(() => {
  console.warn('DictLabel组件将在不久后移除，请使用新组件：https://gitee.com/zuohuaijun/Admin.NET/pulls/1559');
  setDictValue(props.value);
});

watch(
	() => props.value,
	(newValue) => setDictValue(newValue)
);

const setDictValue = (value: any) => {
  state.dict = useUserInfo().dictList[props.code]?.find((x: any) => x[props.propValue] == value);
  if (state.dict != null && !tagTypeMap[state.dict.tagType]) state.dict.tagType = "primary";
}
</script>
