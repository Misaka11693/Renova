<template>
	<div class="sys-import-data-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="400px" @close="resetDialog">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-UploadFilled /> </el-icon>
					<span> 数据导入 </span>
				</div>
			</template>
			
			<el-row :gutter="15" v-loading="state.loading">
				<el-col :span="24" class="mb10">
					<el-button icon="ele-Download" v-reclick="3000" @click="download" :disabled="state.loading">模板</el-button>
				</el-col>
				
				<el-col :span="24" class="mb15 flex">
					<el-upload
						:limit="1"
						:show-file-list="false"
						:auto-upload="false"
						:on-exceed="handleExceed"
						:on-change="handleFileChange"
						ref="uploadRef"
					>
						<template #trigger>
							<el-button class="mr10" type="primary" icon="ele-MostlyCloudy" :disabled="state.isCompleted || state.loading">选择文件</el-button>
						</template>
					</el-upload>
					<span class="selected-file">{{ state.selectedFile ? state.selectedFile.name : '未选择文件' }}</span>
				</el-col>
				
				<!-- 错误提示区域 -->
				<el-col :span="24" v-if="state.importResultUrl" class="mt10">
					<div v-if="state.hasError" style="color: red; margin-bottom: 10px;">
						导入完毕，存在部分错误，请下载导入结果查看详情
					</div>
					<el-link type="primary" :underline="false" @click="downloadImportResult">
						<el-icon class="mr5"><ele-Download /></el-icon>下载导入结果
					</el-link>
				</el-col>
			</el-row>

			<template #footer>
				<span class="dialog-footer">
					<el-button @click="closeDialog" :disabled="state.loading">取 消</el-button>
					<el-button 
						type="primary" 
						@click="state.isCompleted ? closeDialog() : submitImport()"
						:disabled="(!state.selectedFile && !state.isCompleted) || state.loading"
					>
						{{ state.isCompleted ? '关 闭' : '确 定' }}
					</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysImportData">
import type { UploadInstance, UploadProps, UploadRawFile, UploadFile } from 'element-plus'
import { ElUpload, ElMessage, genFileId } from 'element-plus';
import { downloadStreamFile } from '/@/utils/download';
import { reactive, ref } from 'vue';

const uploadRef = ref<UploadInstance>();
const state = reactive({
  isShowDialog: false,
  loading: false,
  isCompleted: false,
  hasError: false,
  selectedFile: null as File | null,
  importResultUrl: '' as string,
  importResultName: '' as string
});

// 定义子组件向父组件传值/事件
const props = defineProps(['import', 'download']);
const emit = defineEmits(['refresh']);

// 打开弹窗
const openDialog = () => {
  resetDialog();
  state.isShowDialog = true;
};

// 重置对话框状态
const resetDialog = () => {
  state.isCompleted = false;
  state.hasError = false;
  state.selectedFile = null;
  state.importResultUrl = '';
  state.importResultName = '';
  uploadRef.value?.clearFiles();
};

// 关闭弹窗
const closeDialog = () => {
  state.isShowDialog = false;
  if (state.importResultUrl) {
    URL.revokeObjectURL(state.importResultUrl);
  }
};

// 选择文件超出上限事件
const handleExceed: UploadProps['onExceed'] = (files) => {
  uploadRef.value!.clearFiles();
  const file = files[0] as UploadRawFile;
  file.uid = genFileId();
  uploadRef.value!.handleStart(file);
}

// 文件选择变更事件
const handleFileChange = (file: UploadFile) => {
  state.selectedFile = file.raw as File;
};

// 提交导入
const submitImport = async () => {
  if (!state.selectedFile) return;
  
  try {
    state.loading = true;
    const res = await props.import(state.selectedFile);
    
    // 处理导入结果
    const contentType = res.headers['content-type'] || '';
    if (contentType.includes('application/json')) {
      // JSON响应处理（无错误文件）
      const decoder = new TextDecoder('utf-8');
      const data = decoder.decode(res.data);
      const result = JSON.parse(data);
      
      if (result.code === 200) {
        ElMessage.success(result.message);
        emit('refresh');
        state.hasError = false;
        closeDialog();  // 关键修改：成功导入后直接关闭对话框
      } else {
        ElMessage.error(result.message);
        state.hasError = false;
      }
    } else {
      // 二进制响应处理（有错误文件）
      const blob = new Blob([res.data]);
      const contentDisposition = res.headers['content-disposition'];
      let filename = '导入结果.xlsx';
      
      if (contentDisposition) {
        const match = contentDisposition.match(/filename="?([^"]+)"?/);
        if (match && match[1]) {
          filename = decodeURIComponent(match[1]);
        }
      }
      
      // 清除旧URL
      if (state.importResultUrl) {
        URL.revokeObjectURL(state.importResultUrl);
      }
      
      // 创建导入结果URL
      state.importResultUrl = URL.createObjectURL(blob);
      state.importResultName = filename;
      state.isCompleted = true;
      state.hasError = true;
      
	  //刷新列表显示
	  emit('refresh');

      ElMessage.warning('导入完成，存在部分错误');
    }
  } catch (error) {
    console.error('导入错误:', error);
    ElMessage.error('导入过程中发生错误');
    state.hasError = false;
  } finally {
    state.loading = false;
  }
};

// 下载导入结果
const downloadImportResult = () => {
  if (!state.importResultUrl) return;
  
  const link = document.createElement('a');
  link.href = state.importResultUrl;
  link.download = state.importResultName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  
  // 关闭对话框（可选，根据需求决定是否保留）
  // closeDialog();
};

// 下载模板
const download = () => {
  props.download()
    .then((res: any) => downloadStreamFile(res))
    .catch((err: any) => ElMessage.error('下载错误: ' + err));
}

// 导出对象
defineExpose({ openDialog, closeDialog });
</script>

<style scoped>
.selected-file {
  margin-left: 10px;
  line-height: 32px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 200px;
}

.flex {
  display: flex;
  align-items: center;
}
</style>