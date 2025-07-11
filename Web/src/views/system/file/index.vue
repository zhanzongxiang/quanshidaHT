<template>
	<div class="sys-file-container">
		<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
			<el-form :model="state.queryParams" ref="queryForm" :inline="true">
				<el-form-item label="租户" v-if="userStore.userInfos.accountType == 999">
					<el-select v-model="state.queryParams.tenantId" placeholder="租户" style="width: 100%">
						<el-option :value="item.value" :label="`${item.label} (${item.host})`" v-for="(item, index) in state.tenantList" :key="index" />
					</el-select>
				</el-form-item>
				<el-form-item label="文件名称" prop="fileName">
					<el-input v-model="state.queryParams.fileName" placeholder="文件名称" clearable />
				</el-form-item>
				<el-form-item label="开始时间" prop="name">
					<el-date-picker v-model="state.queryParams.startTime" type="datetime" placeholder="开始时间" value-format="YYYY-MM-DD HH:mm:ss" />
				</el-form-item>
				<el-form-item label="结束时间" prop="code">
					<el-date-picker v-model="state.queryParams.endTime" type="datetime" placeholder="结束时间" value-format="YYYY-MM-DD HH:mm:ss" />
				</el-form-item>
				<el-form-item>
					<el-button-group>
						<el-button type="primary" icon="ele-Search" @click="handleQuery" v-auth="'sysFile:page'"> 查询 </el-button>
						<el-button icon="ele-Refresh" @click="resetQuery"> 重置 </el-button>
					</el-button-group>
				</el-form-item>
				<el-form-item>
					<el-button type="primary" icon="ele-Plus" @click="openUploadDialog" v-auth="'sysFile:uploadFile'"> 上传 </el-button>
				</el-form-item>
			</el-form>
		</el-card>

		<el-card class="full-table" shadow="hover" style="margin-top: 5px">
			<el-table :data="state.fileData" style="width: 100%" v-loading="state.loading" border>
				<el-table-column type="index" label="序号" width="55" align="center" />
				<el-table-column prop="fileName" label="名称" min-width="150" header-align="center" show-overflow-tooltip />
				<el-table-column prop="suffix" label="后缀" align="center" show-overflow-tooltip>
					<template #default="scope">
						<el-tag round>{{ scope.row.suffix }}</el-tag>
					</template>
				</el-table-column>
				<el-table-column prop="sizeKb" label="大小kb" align="center" show-overflow-tooltip />
				<el-table-column prop="url" label="预览" align="center">
					<template #default="scope">
						<el-image
							style="width: 60px; height: 60px"
							:src="getFileUrl(scope.row)"
							alt="无法预览"
							:lazy="true"
							:hide-on-click-modal="true"
							:preview-src-list="[getFileUrl(scope.row)]"
							:initial-index="0"
							fit="scale-down"
							preview-teleported
						>
							<template #error> </template>
						</el-image>
					</template>
				</el-table-column>
				<el-table-column prop="bucketName" label="存储位置" align="center" show-overflow-tooltip />
				<el-table-column prop="id" label="存储标识" align="center" show-overflow-tooltip />
				<el-table-column prop="fileType" label="文件类型" min-width="100" header-align="center" show-overflow-tooltip />
				<el-table-column prop="isPublic" label="是否公开" min-width="100" header-align="center" show-overflow-tooltip>
					<template #default="scope">
						<el-tag v-if="scope.row.isPublic === true" type="success">是</el-tag>
						<el-tag v-else type="danger">否</el-tag>
					</template>
				</el-table-column>
				<el-table-column prop="relationName" label="关联对象名称" min-width="150" align="center" />
				<el-table-column prop="relationId" label="关联对象Id" align="center" />
				<el-table-column prop="belongId" label="所属Id" align="center" />
				<el-table-column label="修改记录" width="100" align="center" show-overflow-tooltip>
					<template #default="scope">
						<ModifyRecord :data="scope.row" />
					</template>
				</el-table-column>
				<el-table-column label="操作" width="260" fixed="right" align="center" show-overflow-tooltip>
					<template #default="scope">
						<el-button-group>
							<el-button icon="ele-View" size="small" type="primary" @click="openFilePreviewDialog(scope.row)" v-auth="'sysFile:delete'"></el-button>
							<el-button icon="ele-Download" size="small" type="primary" @click="downloadFile(scope.row)" v-auth="'sysFile:downloadFile'"></el-button>
							<el-button icon="ele-Delete" size="small" type="danger" @click="delFile(scope.row)" v-auth="'sysFile:delete'"></el-button>
							<el-button icon="ele-Edit" size="small" type="primary" @click="openEditSysFile(scope.row)" v-auth="'sysFile:update'"></el-button>
						</el-button-group>
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

		<el-dialog v-model="state.dialogUploadVisible" :lock-scroll="false" draggable width="400px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-UploadFilled /> </el-icon>
					<span> 上传文件 </span>
				</div>
			</template>
			<div>
				<el-select v-model="state.fileType" placeholder="请选择文件类型" style="margin-bottom: 10px">
					<el-option label="相关文件" value="相关文件" />
					<el-option label="归档文件" value="归档文件" />
				</el-select>
				是否公开：
				<el-radio-group v-model="state.isPublic">
					<el-radio :value="false">否</el-radio>
					<el-radio :value="true">是</el-radio>
				</el-radio-group>
				<el-upload ref="uploadRef" drag :auto-upload="false" :limit="1" :file-list="state.fileList" action :on-change="handleChange" accept=".jpg,.png,.bmp,.gif,.txt,.xml,.pdf,.xlsx,.docx">
					<el-icon class="el-icon--upload">
						<ele-UploadFilled />
					</el-icon>
					<div class="el-upload__text">将文件拖到此处，或<em>点击上传</em></div>
					<template #tip>
						<div class="el-upload__tip">请上传大小不超过 10MB 的文件</div>
					</template>
				</el-upload>
			</div>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="state.dialogUploadVisible = false">取消</el-button>
					<el-button type="primary" @click="uploadFile">确定</el-button>
				</span>
			</template>
		</el-dialog>

		<el-drawer :title="state.fileName" v-model="state.dialogDocxVisible" size="50%" destroy-on-close>
			<vue-office-docx :src="state.docxUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler" />
		</el-drawer>
		<el-drawer :title="state.fileName" v-model="state.dialogXlsxVisible" size="50%" destroy-on-close>
			<vue-office-excel :src="state.excelUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler" />
		</el-drawer>
		<el-drawer :title="state.fileName" v-model="state.dialogPdfVisible" size="50%" destroy-on-close>
			<vue-office-pdf :src="state.pdfUrl" style="height: 100vh" @rendered="renderedHandler" @error="errorHandler" />
		</el-drawer>
		<el-image-viewer v-if="state.showViewer" :url-list="state.previewList" :hideOnClickModal="true" @close="state.showViewer = false"></el-image-viewer>
		<EditSysFile ref="editSysFileRef" title="编辑文件" @handleQuery="handleQuery" />
	</div>
</template>

<script lang="ts" setup name="sysFile">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage, UploadInstance } from 'element-plus';
import VueOfficeDocx from '@vue-office/docx';
import VueOfficeExcel from '@vue-office/excel';
import VueOfficePdf from '@vue-office/pdf';
import '@vue-office/docx/lib/index.css';
import '@vue-office/excel/lib/index.css';

import EditSysFile from '/@/views/system/file/component/editSysfile.vue';
import ModifyRecord from '/@/components/table/modifyRecord.vue';

import { downloadByUrl } from '/@/utils/download';
import { getAPI } from '/@/utils/axios-utils';
import {SysFileApi, SysTenantApi} from '/@/api-services/api';
import { SysFile } from '/@/api-services/models';
import { useUserInfo } from "/@/stores/userInfo";

// const baseUrl = window.__env__.VITE_API_URL;
const userStore = useUserInfo();
const uploadRef = ref<UploadInstance>();
const editSysFileRef = ref<InstanceType<typeof EditSysFile>>();
const state = reactive({
	loading: false,
	tenantList: [] as Array<any>,
	fileData: [] as Array<SysFile>,
	queryParams: {
		tenantId: undefined,
		fileName: undefined,
		startTime: undefined,
		endTime: undefined,
	},
	tableParams: {
		page: 1,
		pageSize: 50,
		total: 0 as any,
	},
	dialogUploadVisible: false,
	diaglogEditFile: false,
	fileList: [] as any,
	dialogDocxVisible: false,
	dialogXlsxVisible: false,
	dialogPdfVisible: false,
	showViewer: false,
	docxUrl: '',
	excelUrl: '',
	pdfUrl: '',
	fileName: '',
	fileType: '',
	isPublic: false,
	previewList: [] as string[],
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
	if (state.queryParams.startTime == null) state.queryParams.startTime = undefined;
	if (state.queryParams.endTime == null) state.queryParams.endTime = undefined;

	state.loading = true;
	let params = Object.assign(state.queryParams, state.tableParams);
	var res = await getAPI(SysFileApi).apiSysFilePagePost(params);
	console.log(res);
	state.fileData = res.data.result?.items ?? [];
	state.tableParams.total = res.data.result?.total;
	state.loading = false;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.fileName = undefined;
	state.queryParams.startTime = undefined;
	state.queryParams.endTime = undefined;
	handleQuery();
};

// 打开上传页面
const openUploadDialog = () => {
	state.fileList = [];
	state.dialogUploadVisible = true;
	state.isPublic = false;
};

// 通过onChanne方法获得文件列表
const handleChange = (file: any, fileList: []) => {
	state.fileList = fileList;
};

// 上传
const uploadFile = async () => {
	if (state.fileList.length < 1) return;
	await getAPI(SysFileApi).apiSysFileUploadFilePostForm(state.fileList[0].raw, state.fileType, state.isPublic, undefined);
	handleQuery();
	ElMessage.success('上传成功');
	state.dialogUploadVisible = false;
};

// 下载
const downloadFile = async (row: any) => {
	// var res = await getAPI(SysFileApi).sysFileDownloadPost({ id: row.id });
	var fileUrl = getFileUrl(row);
	downloadByUrl({ url: fileUrl });
};

// 删除
const delFile = (row: any) => {
	ElMessageBox.confirm(`确定删除文件：【${row.fileName}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await getAPI(SysFileApi).apiSysFileDeletePost({ id: row.id });
			handleQuery();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

// 打开文件预览页面
const openFilePreviewDialog = async (row: any) => {
	if (row.suffix == '.pdf') {
		state.fileName = `【${row.fileName}${row.suffix}】`;
		state.pdfUrl = getFileUrl(row);
		state.dialogPdfVisible = true;
	} else if (row.suffix == '.docx') {
		state.fileName = `【${row.fileName}${row.suffix}】`;
		state.docxUrl = getFileUrl(row);
		state.dialogDocxVisible = true;
	} else if (row.suffix == '.xlsx') {
		state.fileName = `【${row.fileName}${row.suffix}】`;
		state.excelUrl = getFileUrl(row);
		state.dialogXlsxVisible = true;
	} else if (['.jpg', '.png', '.jpeg', '.bmp'].findIndex((e) => e == row.suffix) > -1) {
		state.previewList = [getFileUrl(row)];
		state.showViewer = true;
	} else {
		ElMessage.error('此文件格式不支持预览');
	}
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

// 获取文件地址
const getFileUrl = (row: SysFile): string => {
	if (row.bucketName == 'Local') {
		return `/${row.filePath}/${row.id}${row.suffix}`;
	} else {
		return row.url!;
	}
};

// 打开编辑页面
const openEditSysFile = (row: any) => {
	editSysFileRef.value?.openDialog(row);
};

// 文件渲染完成
const renderedHandler = () => {};
// 文件渲染失败
const errorHandler = () => {};
</script>
