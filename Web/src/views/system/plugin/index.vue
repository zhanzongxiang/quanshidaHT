<template>
	<div class="sys-plugin-container">
		<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
			<el-form :model="state.queryParams" ref="queryForm" :inline="true">
				<el-form-item label="租户" v-if="userStore.userInfos.accountType == 999">
					<el-select v-model="state.queryParams.tenantId" placeholder="租户" style="width: 100%">
						<el-option :value="item.value" :label="`${item.label} (${item.host})`" v-for="(item, index) in state.tenantList" :key="index" />
					</el-select>
				</el-form-item>
				<el-form-item label="功能名称">
					<el-input v-model="state.queryParams.name" placeholder="功能名称" clearable />
				</el-form-item>
				<el-form-item>
					<el-button-group>
						<el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'sysPlugin:page'"> 查询 </el-button>
						<el-button icon="ele-Refresh" @click="resetQuery"> 重置 </el-button>
					</el-button-group>
				</el-form-item>
				<el-form-item>
					<el-button type="primary" icon="ele-Plus" @click="openAddPlugin" v-auth="'sysPlugin:add'"> 新增 </el-button>
				</el-form-item>
			</el-form>
		</el-card>

		<el-card class="full-table" shadow="hover" style="margin-top: 5px">
			<el-table :data="state.pluginData" style="width: 100%" v-loading="state.loading" border>
				<el-table-column type="index" label="序号" width="55" align="center" fixed />
				<el-table-column prop="name" label="功能名称" header-align="center" show-overflow-tooltip />
				<el-table-column prop="assemblyName" label="程序集名称" header-align="center" show-overflow-tooltip />
				<el-table-column prop="orderNo" label="排序" width="70" align="center" show-overflow-tooltip />
				<el-table-column label="状态" width="70" align="center" show-overflow-tooltip>
					<template #default="scope">
            <g-sys-dict v-model="scope.row.status" code="StatusEnum" />
					</template>
				</el-table-column>
				<el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
					<template #default="scope">
						<ModifyRecord :data="scope.row" />
					</template>
				</el-table-column>
				<el-table-column label="操作" width="140" fixed="right" align="center" show-overflow-tooltip>
					<template #default="scope">
						<el-button icon="ele-Edit" size="small" text type="primary" @click="openEditPlugin(scope.row)" v-auth="'sysPlugin:update'"> 编辑 </el-button>
						<el-button icon="ele-Delete" size="small" text type="danger" @click="delPlugin(scope.row)" v-auth="'sysPlugin:delete'"> 删除 </el-button>
					</template>
				</el-table-column>
			</el-table>
			<el-pagination
				v-model:currentPage="state.tableParams.page"
				v-model:page-size="state.tableParams.pageSize"
				:total="state.tableParams.total"
				:page-sizes="[10, 20, 50, 100]"
				size="small"
				background
				@size-change="handleSizeChange"
				@current-change="handleCurrentChange"
				layout="total, sizes, prev, pager, next, jumper"
			/>
		</el-card>

		<EditPlugin ref="editPluginRef" :title="state.editPluginTitle" @handleQuery="handleQuery" />
	</div>
</template>

<script lang="ts" setup name="sysPlugin">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import EditPlugin from '/@/views/system/plugin/component/editPlugin.vue';
import ModifyRecord from '/@/components/table/modifyRecord.vue';
import { getAPI } from '/@/utils/axios-utils';
import { SysPluginApi, SysTenantApi } from '/@/api-services/api';
import { SysPlugin } from '/@/api-services/models';
import { useUserInfo } from "/@/stores/userInfo";

const userStore = useUserInfo();
const editPluginRef = ref<InstanceType<typeof EditPlugin>>();
const state = reactive({
	loading: false,
	tenantList: [] as Array<any>,
	pluginData: [] as Array<SysPlugin>,
	queryParams: {
		tenantId: undefined,
		name: undefined,
	},
	tableParams: {
		page: 1,
		pageSize: 50,
		total: 0 as any,
	},
	editPluginTitle: '',
});

onMounted(async () => {
	if (userStore.userInfos.accountType == 999) {
		state.tenantList = await getAPI(SysTenantApi).apiSysTenantListGet().then(res => res.data.result ?? []);
		state.queryParams.tenantId = userStore.userInfos.currentTenantId as any;
	}
	handleQuery();
});

// 查询操作
const handleQuery = async () => {
	state.loading = true;
	let params = Object.assign(state.queryParams, state.tableParams);
	var res = await getAPI(SysPluginApi).apiSysPluginPagePost(params);
	state.pluginData = res.data.result?.items ?? [];
	state.tableParams.total = res.data.result?.total;
	state.loading = false;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.name = undefined;
	handleQuery();
};

// 打开新增页面
const openAddPlugin = () => {
	state.editPluginTitle = '添加动态插件';
	editPluginRef.value?.openDialog({ orderNo: 100, tenantId: state.queryParams.tenantId, status: 1 });
};

// 打开编辑页面
const openEditPlugin = (row: any) => {
	state.editPluginTitle = '编辑动态插件';
	editPluginRef.value?.openDialog(row);
};

// 删除当前行
const delPlugin = (row: any) => {
	ElMessageBox.confirm(`确定删除动态插件：【${row.name}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await getAPI(SysPluginApi).apiSysPluginDeletePost({ id: row.id });
			handleQuery();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

// 改变页面容量
const handleSizeChange = (val: number) => {
	state.tableParams.pageSize = val;
	handleQuery();
};

// 改变页码序号
const handleCurrentChange = (val: number) => {
	state.tableParams.page = val;
	handleQuery();
};
</script>
