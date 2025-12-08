<!-- 组件使用文档： https://gitee.com/zuohuaijun/Admin.NET/pulls/1559  -->
<script setup lang="ts">
import { reactive, watch, PropType } from 'vue';
import { useUserInfo } from '/@/stores/userInfo';

type DictItem = {
  [key: string]: any;
  tagType?: string;
  styleSetting?: string;
  classSetting?: string;
};

const userStore = useUserInfo();
const emit = defineEmits(['update:modelValue']);
const props = defineProps({
  /**
   * 绑定的值，支持多种类型
   * @example
   * <g-sys-dict v-model="selectedValue" code="xxxx" />
   */
  modelValue: {
    type: [String, Number, Boolean, Array, null] as PropType<string | number | boolean | any[] | null>,
    default: null,
    required: true,
  },
  /**
   * 字典编码，用于获取字典项
   * @example 'gender'
   */
  code: {
    type: String,
    required: true,
  },
  /**
   * 是否是常量
   * @default false
   */
  isConst: {
    type: Boolean,
    default: false,
  },
  /**
   * 字典项中用于显示的字段名
   * @default 'label'
   */
  propLabel: {
    type: String,
    default: 'label',
  },
  /**
   * 字典项中用于取值的字段名
   * @default 'value'
   */
  propValue: {
    type: String,
    default: 'value',
  },
  /**
   * 字典项过滤函数
   * @param dict - 字典项
   * @returns 是否保留该项
   * @default (dict) => true
   */
  onItemFilter: {
    type: Function as PropType<(dict: DictItem) => boolean>,
    default: (dict: DictItem) => true,
  },
  /**
   * 字典项显示内容格式化函数
   * @param dict - 字典项
   * @returns 格式化后的显示内容
   * @default () => undefined
   */
  onItemFormatter: {
    type: Function as PropType<(dict: DictItem) => string | undefined | null>,
    default: () => undefined,
  },
  /**
   * 组件渲染方式
   * @values 'tag', 'select', 'radio', 'checkbox'
   * @default 'tag'
   */
  renderAs: {
    type: String as PropType<'tag' | 'select' | 'radio' | 'checkbox'>,
    default: 'tag',
    validator(value: string) {
      return ['tag', 'select', 'radio', 'checkbox'].includes(value);
    },
  },
  /**
   * 是否多选
   * @default false
   */
  multiple: {
    type: Boolean,
    default: false,
  },
});

const state = reactive({
  dict: undefined as DictItem | DictItem[] | undefined,
  dictData: [] as DictItem[],
  value: undefined as any,
});

// 获取数据集
const getDataList = () => {
  if (props.isConst) {
    const data = userStore.constList?.find((x: any) => x.code === props.code)?.data?.result ?? [];
    // 与字典的显示文本、值保持一致，方便渲染
    data?.forEach((item: any) => {
      item.label = item.name;
      item.value = item.code;
      delete item.name;
    });
    return data;
  } else {
    return userStore.dictList[props.code];
  }
}

// 设置字典数据
const setDictData = () => {
  state.dictData = getDataList()?.filter(props.onItemFilter) ?? [];
  processNumericValues(props.modelValue);
};

// 处理数字类型的值
const processNumericValues = (value: any) => {
  if (typeof value === 'number' || (Array.isArray(value) && typeof value[0] === 'number')) {
    state.dictData.forEach((item) => {
      item[props.propValue] = Number(item[props.propValue]);
    });
  }
};

// 设置多选值
const trySetMultipleValue = (value: any) => {
  let newValue = value;
  if (typeof value === 'string') {
    const trimmedValue = value.trim();
    if (trimmedValue.startsWith('[') && trimmedValue.endsWith(']')) {
      try {
        newValue = JSON.parse(trimmedValue);
      } catch (error) {
        console.warn('[g-sys-dict]解析多选值失败, 异常信息:', error);
      }
    }
  } else if (props.multiple && !value) {
    newValue = [];
  }
  if (newValue != value) updateValue(newValue);

  setDictData();
  return newValue;
}

// 设置字典值
const setDictValue = (value: any) => {
  value = trySetMultipleValue(value);
  if (Array.isArray(value)) {
    state.dict = state.dictData?.filter((x) => value.find(y => y == x[props.propValue]));
    state.dict?.forEach(ensureTagType);
  } else {
    state.dict = state.dictData?.find((x) => x[props.propValue] == value);
    if (state.dict) ensureTagType(state.dict);
  }
  state.value = value;
};

// 确保标签类型存在
const ensureTagType = (item: DictItem) => {
  if (!['success', 'warning', 'info', 'primary', 'danger'].includes(item.tagType ?? '')) {
    item.tagType = 'primary';
  }
};

// 更新绑定值
const updateValue = (newValue: any) => {
  emit('update:modelValue', newValue);
};

// 计算显示的文本
const getDisplayText = (dict: DictItem | undefined = undefined) => {
  if (dict) return props.onItemFormatter?.(dict) ?? dict[props.propLabel];
  return state.value;
}

watch(
    () => props.modelValue,
    (newValue) => setDictValue(newValue),
    { immediate: true }
);
</script>

<template>
  <!-- 渲染标签 -->
  <template v-if="props.renderAs === 'tag'">
    <template v-if="Array.isArray(state.dict)">
      <el-tag v-for="(item, index) in state.dict" :key="index" v-bind="$attrs" :type="item.tagType" :style="item.styleSetting" :class="item.classSetting" class="mr2">
        {{ getDisplayText(item) }}
      </el-tag>
    </template>
    <template v-else>
      <el-tag v-if="state.dict" v-bind="$attrs" :type="state.dict.tagType" :style="state.dict.styleSetting" :class="state.dict.classSetting">
        {{ getDisplayText(state.dict) }}
      </el-tag>
      <span v-else>{{ getDisplayText() }}</span>
    </template>
  </template>

  <!-- 渲染选择器 -->
  <template v-if="props.renderAs === 'select'">
    <el-select v-model="state.value" v-bind="$attrs" :multiple="props.multiple" @change="updateValue" clearable>
      <el-option v-for="(item, index) in state.dictData" :key="index" :label="getDisplayText(item)" :value="item[propValue]" />
    </el-select>
  </template>

  <!-- 渲染复选框（多选） -->
  <template v-if="props.renderAs === 'checkbox'">
    <el-checkbox-group v-model="state.value" v-bind="$attrs" @change="updateValue">
      <el-checkbox-button v-for="(item, index) in state.dictData" :key="index" :value="item[propValue]">
        {{ getDisplayText(item) }}
      </el-checkbox-button>
    </el-checkbox-group>
  </template>

  <!-- 渲染单选框 -->
  <template v-if="props.renderAs === 'radio'">
    <el-radio-group v-model="state.value" v-bind="$attrs" @change="updateValue">
      <el-radio v-for="(item, index) in state.dictData" :key="index" :value="item[propValue]">
        {{ getDisplayText(item) }}
      </el-radio>
    </el-radio-group>
  </template>
</template>
<style scoped lang="scss">
</style>