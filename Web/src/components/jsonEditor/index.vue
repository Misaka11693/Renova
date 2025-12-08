<!--
// JsonEditor组件，用于编辑类json格式数据，防止配置式数据的错误格式输入
// 使用示例：
<template>
	<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
		<el-form-item label="内容" prop="content">
			<JsonEditor ref="jsonEditorRef" v-model:jsonObj="ruleForm.content"></JsonEditor>
		</el-form-item>
	</el-col>
</template>
<script lang="ts" setup>
import JsonEditor from '/@/components/jsonEditor/index.vue';
</script>
-->

<template>
	<!-- 根据 isJsonValid 控制 JsonEditorVue 的显示 -->
	<JsonEditorVue ref="jsonEditorVueRef" v-show="isJsonValid" v-model="internalJsonObj" mode="text" style="width: 100%" />
</template>

<script setup lang="ts" name="jsonEditor">
import { ref, useTemplateRef, watch, onMounted, computed } from 'vue';
import JsonEditorVue from 'json-editor-vue';

const props = defineProps({
	jsonObj: {
		type: null,
		default: null, // 允许为 null
		required: false, // 不是必需的
	},
});
const jsonEditorVueRef = useTemplateRef('jsonEditorVueRef');
const internalJsonObj = ref(props.jsonObj);
const emit = defineEmits(['update:jsonObj']);

// 计算属性，判断 jsonObj 是否为有效的 JSON 字符串，这里为简易判断，防止输入框失去焦点时，组件频繁隐藏和显示
const isJsonValid = computed(() => {
	try {
		if (internalJsonObj.value && (internalJsonObj.value.startsWith('{') || internalJsonObj.value.startsWith('[')) && (internalJsonObj.value.endsWith('}') || internalJsonObj.value.endsWith(']'))) {
			return true;
		}
	} catch {
		return false;
	}
	return false;
});

// 监听内部 JSON 对象变化并触发外部更新
watch(internalJsonObj, () => {
	emit('update:jsonObj', internalJsonObj.value);
});

// 监听外部字符串变化并更新内部JSON对象
watch(
	() => props.jsonObj,
	(newVal) => {
		internalJsonObj.value = newVal;
	}
);

onMounted(() => {
	jsonEditorVueRef.value.jsonEditor.focus();
});
</script>
