<template>
    <div class="multi-lang-input">
        <el-input v-model="inputModelValue" :placeholder="`请输入 ${currentLangLabel}`" clearable
            @update:model-value="(val: string) => emit('update:modelValue', val)">
            <template #append>
                <el-button @click="openDialog" circle>
                    <template #icon>
                        <i class="iconfont icon-diqiu1"></i>
                    </template>
                </el-button>
            </template>
        </el-input>

        <el-dialog v-model="dialogVisible" title="多语言设置" draggable :close-on-click-modal="false" width="600px">
            <el-form ref="ruleFormRef" label-width="auto">
                <el-row :gutter="35">
                    <el-col v-for="lang in languages" :key="lang.code" :span="24" class="mb10">
                        <el-form-item :label="lang.label">
                            <el-input v-model="multiLangValue[lang.code]" :placeholder="`请输入: ${lang.label}`"
                                clearable />
                        </el-form-item>
                    </el-col>
                </el-row>
            </el-form>

            <template #footer>
                <el-button @click="aiTranslation">AI翻译</el-button>
                <el-button @click="closeDialog">关闭</el-button>
                <el-button type="primary" @click="confirmDialog">确认修改</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useLangStore } from "/@/stores/useLangStore";
import { Local } from "/@/utils/storage";
import { getAPI } from "/@/utils/axios-utils";
import { SysLangTextApi } from "/@/api-services/api";
import { ElMessage } from "element-plus";

const emit = defineEmits<{
    (e: "update:modelValue", value?: string | null): void;
}>();

const ruleFormRef = ref();

const props = defineProps<{
    modelValue?: string | null;
    entityName: string;
    entityId: number;
    fieldName: string;
}>();

const inputModelValue = computed({
    get: () => props.modelValue,
    set: (val) => emit("update:modelValue", val),
});
const langStore = useLangStore();
const languages = ref<any[]>([]);

const currentLang = ref("zh-CN");
const activeLang = ref("zh-CN");
const dialogVisible = ref(false);
const multiLangValue = ref<Record<string, string>>({});

const currentLangLabel = computed(() => {
    return (
        languages.value.find((l) => l.code === currentLang.value)?.label ||
        currentLang.value
    );
});

const fetchMultiLang = async () => {
    const result = await getAPI(SysLangTextApi)
        .apiSysLangTextListPost({
            entityName: props.entityName,
            entityId: props.entityId,
            fieldName: props.fieldName,
            pageSize: 200,
        })
        .then((res) => res.data.result);
    return result ?? [];
};

onMounted(async () => {
    if (langStore.languages.length === 0) {
        await langStore.loadLanguages();
    }
    languages.value = langStore.languages;

    const themeConfig = Local.get("themeConfig");
    const globalI18n = themeConfig?.globalI18n;

    if (globalI18n) {
        const matched = langStore.languages.find(
            (l) => l.code === globalI18n || l.value === globalI18n
        );
        const langCode = matched?.code || "zh-CN";
        currentLang.value = langCode;
        activeLang.value = langCode;
    } else if (languages.value.length > 0) {
        currentLang.value = languages.value[0].code;
        activeLang.value = languages.value[0].code;
    }
});

const aiTranslation = async () => {
    for (const element of languages.value) {
        if (element.code === currentLang.value) continue;
        multiLangValue.value[element.code] = "正在翻译...";
        try {
            const text = await getAPI(SysLangTextApi)
                .apiSysLangTextAiTranslateTextPost({
                    originalText: props.modelValue,
                    targetLang: element.code,
                })
                .then((res) => res.data.result);

            multiLangValue.value[element.code] = text || "";
        } catch (e: any) {
            multiLangValue.value[element.code] = "";
            ElMessage.warning(e.message);
        }
    }
};

const openDialog = async () => {
    if (!props.entityId) {
        ElMessage.warning("请先保存数据！");
        return;
    }
    const res = await fetchMultiLang();

    const newValues: Record<string, string> = {};
    res.forEach((element: { langCode?: string | null; content?: string | null }) => {
        if (element.langCode) {
            newValues[element.langCode] = element.content ?? "";
        }
    });

    newValues[currentLang.value] = props.modelValue ?? "";

    multiLangValue.value = newValues;
    dialogVisible.value = true;
    ruleFormRef.value?.resetFields();
};

const closeDialog = () => {
    dialogVisible.value = false;
    multiLangValue.value = {};
    ruleFormRef.value?.resetFields();
};

const confirmDialog = async () => {
    const langItems = Object.entries(multiLangValue.value)
        .filter(([_, content]) => content && content.trim() !== "")
        .map(([code, content]) => ({
            entityName: props.entityName,
            entityId: props.entityId,
            fieldName: props.fieldName,
            langCode: code,
            content: content,
        }));

    if (langItems.length === 0) {
        ElMessage.warning("请输入至少一条多语言内容！");
        return;
    }

    try {
        await getAPI(SysLangTextApi).apiSysLangTextBatchSavePost(langItems);
        ElMessage.success("保存成功！");
        emit(
            "update:modelValue",
            multiLangValue.value[currentLang.value] ?? props.modelValue
        );
        dialogVisible.value = false;
    } catch (err) {
        console.error(err);
        ElMessage.error("保存失败！");
    }
    dialogVisible.value = false;
    ruleFormRef.value?.resetFields();
};
</script>

<style lang="scss" scoped>
.multi-lang-input {
    width: 100%;
}

.mb10:last-child {
    margin-bottom: 0 !important;
}
</style>
