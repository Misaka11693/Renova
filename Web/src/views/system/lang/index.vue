<script lang="ts" setup name="sysLang">
import { ref, reactive, onMounted } from "vue";
import { auth } from '/@/utils/authFunction';
import { ElMessageBox, ElMessage } from "element-plus";
import { getAPI } from '/@/utils/axios-utils';
import { SysLangApi } from '/@/api-services/api';
import editDialog from '/@/views/system/lang/component/editDialog.vue'
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import { SysLangOutput } from '/@/api-services/models/sys-lang-output';

const editDialogRef = ref();
const state = reactive({
  exportLoading: false,
  tableLoading: false,
  stores: {},
  showAdvanceQueryUI: false,
  dropdownData: {} as any,
  selectData: [] as any[],
  tableQueryParams: {} as any,
  tableParams: {
    page: 1,
    pageSize: 20,
    total: 0,
    field: 'active', // 默认的排序字段
    order: 'descending', // 排序方向
    descStr: 'descending', // 降序排序的关键字符
  },
  tableData: [] as SysLangOutput[],
});

// 页面加载时
onMounted(async () => {
});

// 查询操作
const handleQuery = async (params: any = {}) => {
  state.tableLoading = true;
  state.tableParams = Object.assign(state.tableParams, params);
  const result = await  getAPI(SysLangApi).apiSysLangPagePost(Object.assign(state.tableQueryParams, state.tableParams)).then(res => res.data.result);
  state.tableParams.total = result?.total ?? 0;
  state.tableData = result?.items ?? [];
  state.tableLoading = false;
};

// 列排序
const sortChange = async (column: any) => {
  state.tableParams.field = column.prop;
  state.tableParams.order = column.order;
  await handleQuery();
};

// 删除
const delSysLang = (row: any) => {
  ElMessageBox.confirm(`确定要删除吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    await getAPI(SysLangApi).apiSysLangDeletePost({ id: row.id });
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

const batchDelSysLang = () => {
  ElMessageBox.confirm(`确定要删除选中的 ${state.selectData.length} 条记录吗?`, "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    const ids = state.selectData.map((item) => item.id);
    //await getAPI(SysLangApi).apiSysLangBatchDeletePost({ ids });
    state.selectData = [];
    handleQuery();
    ElMessage.success("删除成功");
  }).catch(() => {});
};

handleQuery();
</script>
<template>
  <div class="sysLang-container" v-loading="state.exportLoading">
    <el-card shadow="hover" :body-style="{ paddingBottom: '0' }"> 
      <el-form :model="state.tableQueryParams" ref="queryForm" labelWidth="90">
        <el-row>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item label="关键字">
              <el-input v-model="state.tableQueryParams.keyword" clearable placeholder="请输入模糊查询关键字"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="语言名称">
              <el-input v-model="state.tableQueryParams.name" clearable placeholder="请输入语言名称"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="语言代码">
              <el-input v-model="state.tableQueryParams.code" clearable placeholder="请输入语言代码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="ISO 语言代码">
              <el-input v-model="state.tableQueryParams.isoCode" clearable placeholder="请输入ISO 语言代码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="URL 语言代码">
              <el-input v-model="state.tableQueryParams.urlCode" clearable placeholder="请输入URL 语言代码"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10" v-if="state.showAdvanceQueryUI">
            <el-form-item label="是否启用">
              <el-input v-model="state.tableQueryParams.active" clearable placeholder="请输入是否启用"/>
            </el-form-item>
          </el-col>
          <el-col :xs="24" :sm="12" :md="12" :lg="8" :xl="4" class="mb10">
            <el-form-item >
              <el-button-group style="display: flex; align-items: center;">
                <el-button type="primary"  icon="ele-Search" @click="handleQuery" v-auth="'sysLang:page'" v-reclick="1000"> 查询 </el-button>
                <el-button type="danger" style="margin-left:5px;" icon="ele-Delete" @click="batchDelSysLang" :disabled="state.selectData.length == 0" v-auth="'sysLang:batchDelete'"> 删除 </el-button>
                <el-button type="primary" style="margin-left:5px;" icon="ele-Plus" @click="editDialogRef.openDialog(null, '新增多语言')" v-auth="'sysLang:add'"> 新增 </el-button>
              </el-button-group>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-card>
    <el-card class="full-table" shadow="hover" style="margin-top: 5px">
      <el-table :data="state.tableData" style="width: 100%" v-loading="state.tableLoading" tooltip-effect="light" row-key="id" @sort-change="sortChange" border>
        <el-table-column type="index" label="序号" width="55" align="center"/>
        <el-table-column prop='name' label='语言名称' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='code' label='语言代码' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='isoCode' label='ISO 语言代码' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='urlCode' label='URL 语言代码' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='direction' label='书写方向' sortable='custom' show-overflow-tooltip  >
          <template #default="scope">
						<g-sys-dict v-model="scope.row.direction" code="DirectionEnum" />
					</template>
        </el-table-column>
        <el-table-column prop='dateFormat' label='日期格式' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='timeFormat' label='时间格式' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='weekStart' label='每周起始日' sortable='custom' show-overflow-tooltip >
          <template #default="scope">
						<g-sys-dict v-model="scope.row.weekStart" code="WeekEnum" />
					</template>
        </el-table-column>
        <el-table-column prop='grouping' label='分组符号' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='decimalPoint' label='小数点符号' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='thousandsSep' label='千分位分隔符' sortable='custom' show-overflow-tooltip />
        <el-table-column prop='active' label='是否启用' sortable='custom' show-overflow-tooltip>
          <template #default="scope">
            <el-tag v-if="scope.row.active"> 是 </el-tag>
            <el-tag type="danger" v-else> 否 </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
          <template #default="scope">
            <ModifyRecord :data="scope.row" />
          </template>
        </el-table-column>
        <el-table-column label="操作" width="140" align="center" fixed="right" show-overflow-tooltip v-if="auth('sysLang:update') || auth('sysLang:delete')">
          <template #default="scope">
            <el-button icon="ele-Edit" size="small" text type="primary" @click="editDialogRef.openDialog(scope.row, '编辑多语言')" v-auth="'sysLang:update'"> 编辑 </el-button>
            <el-button icon="ele-Delete" size="small" text type="primary" @click="delSysLang(scope.row)" v-auth="'sysLang:delete'"> 删除 </el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination 
              v-model:currentPage="state.tableParams.page"
              v-model:page-size="state.tableParams.pageSize"
              @size-change="(val: any) => handleQuery({ pageSize: val })"
              @current-change="(val: any) => handleQuery({ page: val })"
              layout="total, sizes, prev, pager, next, jumper"
              :page-sizes="[10, 20, 50, 100, 200, 500]"
              :total="state.tableParams.total"
              size="small"
              background />
      <editDialog ref="editDialogRef" @reloadTable="handleQuery" />
    </el-card>
  </div>
</template>
<style scoped>
:deep(.el-input), :deep(.el-select), :deep(.el-input-number) {
  width: 100%;
}
</style>