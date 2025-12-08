<template>
    <el-card v-bind="$attrs" :class="[sizeClass, fullHeight ? 'full-height' : '']">
        <template #header v-if="title || $slots.prefix || prefixIcon || $slots.suffix || suffix">
            <div class="cardpro-header">
                <div v-if="$slots.prefix || prefixIcon" class="cardpro-header-prefix">
                    <slot v-if="$slots.prefix" name="prefix" />
                    <SvgIcon v-else-if="prefixIcon" :name="prefixIcon" />
                </div>
                <div class="cardpro-header-title">
                    <span>{{ props.title }}</span>
                </div>
                <div v-if="$slots.suffix || suffix" class="cardpro-header-suffix">
                    <slot v-if="$slots.suffix" name="suffix" />
                    <span v-else-if="props.suffix">{{ props.suffix }}</span>
                </div>
            </div>
        </template>
        
        <template v-for="(value, name) in $slots" #[name]="scopedData">
            <slot :name="name" v-bind="scopedData"></slot>
        </template>
    </el-card>
</template>

<script setup lang="ts" name="CardPro">
import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useSizeProp } from 'element-plus'
import { useThemeConfig } from '/@/stores/themeConfig';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const props = defineProps({
    title: { type: String },
    prefixIcon: { type: String },
    suffix: { type: String },
    size: useSizeProp,
    fullHeight: { type: Boolean, default: false }
});

// 获取全局组件大小
const getGlobalComponentSize = computed(() => {
	return themeConfig.value.globalComponentSize;
});

const sizeClass = computed(() => {
    if(props.size) return  "cardpro-" + props.size;
    else return "cardpro-" + getGlobalComponentSize.value;
});
</script>

<style lang="scss" scoped>
.cardpro-header {
    display: flex;
    align-items: center;

    &-prefix {
        height: 100%;
        margin-right: 5px;
        display: flex;
        align-items: center;
        margin-top: 1px;
    }
    &-title { 
        flex: 1; 
        padding: var(--cardpro-padding) 0; 
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
    }
    &-suffix {
        height: 100%;
        margin-left: 5px;
    }
}

.cardpro-small {
    --cardpro-padding: 10px
}
.cardpro-default {
    --cardpro-padding: 15px
}
.cardpro-large {
    --cardpro-padding: 20px
}

:deep(.el-card__header) {
    padding: 0 var(--cardpro-padding);
}
:deep(.el-card__body) {
    padding: var(--cardpro-padding);
}

.full-height {
    display: flex;
    flex-direction:column;
    height: 100%;
    
    :deep(.el-card__body) {
        flex: 1;
    }
}
</style>
