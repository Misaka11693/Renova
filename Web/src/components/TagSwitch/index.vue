<template>
    <div class="tagsw">
        <el-switch size="small" class="tagsw-switch" 
            v-model="tgswModelValue"
            :active-value="activeValue"
            :inactive-value="inactiveValue" 
            @change="handleChange"
        />
        <span class="tagsw-tage">
            <GSysDict v-model="tgswModelValue" :code="code" />
        </span>
    </div>
</template>

<script lang="ts" setup name="TagSwitch">
import { computed, PropType } from 'vue';
import GSysDict from "/@/components/sysDict/sysDict.vue";

const emit = defineEmits(['change', 'update:modelValue']);

const props = defineProps({
    /**
     * 绑定的值，支持多种类型
     * @example
     * <tag-switch v-model="value" code="xxxx" />
   */
    modelValue: {
        type: [String, Number, Boolean, Array, null] as PropType<string | number | boolean | any[] | null>,
        default: null,
        required: true,
    },

    /**
     * 字典编码，用于获取字典项 (同sys-dict)
     * @example 'gender'
   */
    code: {
        type: String,
        required: true,
    },
    
    /**
     * switch 状态为 on 时的值，默认true (同el-switch)
     * @example true
     */
    activeValue: {
        type: [String, Number, Boolean] as PropType<string | number | boolean>,
        default: true,
    },
    /**
     * switch的状态为 off 时的值，默false (同el-switch)
     * @example false
     */
    inactiveValue: {
        type: [String, Number, Boolean] as PropType<string | number | boolean>,
        default: false,
    },
});
const tgswModelValue = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val),
});


const handleChange = (val: any) => {
    emit('change', val);
};
</script>

<style lang="scss" scoped>
.tagsw {
    //width: 100%;

    .tagsw-switch {
        display: none;
        height: 100%;
        line-height: 100%;
    }

    .tagsw-tage {
        display: block;
    }
}
.tagsw:hover {
    .tagsw-switch {
        display: inline-flex;
    }

    .tagsw-tage {
        display: none;
    }
}
</style>
