<template>
	<div class="sys-editCodeGen-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="库定位器" prop="configId" :rules="[{ required: true, message: '请选择库定位器', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.configId" placeholder="库名" filterable @change="dbChanged()" class="w100">
								<el-option v-for="item in state.dbData" :key="item.configId" :label="item.dbNickName" :value="item.configId" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="库类型" prop="dbType" :rules="[{ required: true, message: '描述不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.dbType" placeholder="数据库类型" clearable disabled class="w100">
								<el-option label="MySql" :value="'0'" />
								<el-option label="SqlServer" :value="'1'" />
								<el-option label="Sqlite" :value="'2'" />
								<el-option label="Oracle" :value="'3'" />
								<el-option label="PostgreSQL" :value="'4'" />
								<el-option label="Dm" :value="'5'" />
								<el-option label="Kdbndp" :value="'6'" />
								<el-option label="Oscar" :value="'7'" />
								<el-option label="MySqlConnector" :value="'8'" />
								<el-option label="Access" :value="'9'" />
								<el-option label="OpenGauss" :value="'10'" />
								<el-option label="QuestDB" :value="'11'" />
								<el-option label="HG" :value="'12'" />
								<el-option label="ClickHouse" :value="'13'" />
								<el-option label="GBase" :value="'14'" />
								<el-option label="Odbc" :value="'15'" />
								<el-option label="OceanBaseForOracle" :value="'16'" />
								<el-option label="TDengine" :value="'17'" />
								<el-option label="GaussDB" :value="'18'" />
								<el-option label="OceanBase" :value="'19'" />
								<el-option label="Tidb" :value="'20'" />
								<el-option label="Vastbase" :value="'21'" />
								<el-option label="PolarDB" :value="'22'" />
								<el-option label="Doris" :value="'23'" />
								<el-option label="Custom" :value="'900'" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="库地址" prop="connectionString" :rules="[{ required: true, message: '库地址不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.connectionString" disabled clearable type="textarea" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="生成表" prop="tableName" :rules="[{ required: true, message: '生成表不能为空', trigger: 'blur' }]">
							<el-select v-model="state.ruleForm.tableName" @change="tableChanged" value-key="value" filterable clearable class="w100">
								<el-option v-for="item in state.tableData" :key="item.entityName" :label="item.entityName + ' ( ' + item.tableName + ' ) [' + item.tableComment + ']'" :value="item" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="业务名" prop="busName" :rules="[{ required: true, message: '业务名不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.busName" placeholder="请输入" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="生成菜单" prop="generateMenu">
							<el-radio-group v-model="state.ruleForm.generateMenu">
								<el-radio :value="true">是</el-radio>
								<el-radio :value="false">否</el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="菜单图标" prop="menuIcon">
							<IconSelector v-model="state.ruleForm.menuIcon" :size="getGlobalComponentSize" placeholder="菜单图标" type="all" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="父级菜单" prop="menuPid">
							<el-cascader
								:options="state.menuData"
								:props="cascaderProps"
								placeholder="请选择上级菜单"
								:disabled="!state.ruleForm.generateMenu"
								filterable
								clearable
								class="w100"
								v-model="state.ruleForm.menuPid"
								@change="menuChange"
							>
								<template #default="{ node, data }">
									<span>{{ data.title }}</span>
									<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
								</template>
							</el-cascader>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="命名空间" prop="nameSpace" :rules="[{ required: true, message: '请选择命名空间', trigger: 'blur' }]">
							<!-- <el-input v-model="state.ruleForm.nameSpace" clearable placeholder="请输入" /> -->
							<el-select v-model="state.ruleForm.nameSpace" filterable clearable class="w100" placeholder="命名空间">
								<el-option v-for="(item, index) in props.applicationNamespaces" :key="index" :label="item" :value="item" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="前端目录" prop="pagePath" :rules="[{ required: true, message: '前端目录不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.pagePath" clearable placeholder="请输入" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="作者姓名" prop="authorName">
							<el-input v-model="state.ruleForm.authorName" clearable placeholder="请输入" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="生成方式" prop="generateType">
              <g-sys-dict v-model="state.ruleForm.generateType" code="code_gen_create_type" render-as="select" class="w100" filterable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="支持打印" prop="printType">
              <g-sys-dict v-model="state.ruleForm.printType" code="code_gen_print_type" render-as="select" @change="printTypeChanged" class="w100" filterable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20" v-if="state.ruleForm.printType == 'custom'">
						<el-form-item label="打印模版" prop="printName">
							<el-select v-model="state.ruleForm.printName" filterable class="w100">
								<el-option v-for="item in state.printList" :key="item.id" :label="item.name" :value="item.name" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-divider border-style="dashed" content-position="center">
						<div style="color: #b1b3b8">数据唯一性配置</div>
					</el-divider>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-button icon="ele-Plus" type="primary" plain @click="() => state.ruleForm.tableUniqueList?.push({})"> 增加配置 </el-button>
						<span style="font-size: 12px; color: gray; padding-left: 5px"> 保证字段值的唯一性，排除null值 </span>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<template v-if="state.ruleForm.tableUniqueList != undefined && state.ruleForm.tableUniqueList.length > 0">
							<el-row :gutter="35" v-for="(v, k) in state.ruleForm.tableUniqueList" :key="k">
								<el-col :xs="24" :sm="14" :md="14" :lg="14" :xl="14" class="mb20">
									<el-form-item label="字段" :prop="`tableUniqueList[${k}].columns`" :rules="[{ required: true, message: `字段不能为空`, trigger: 'blur' }]">
										<template #label>
											<el-button icon="ele-Delete" type="danger" circle plain size="small" @click="() => state.ruleForm.tableUniqueList?.splice(k, 1)" />
											<span class="ml5">字段</span>
										</template>
										<el-select v-model="state.ruleForm.tableUniqueList[k].columns" @change="(val: any) => changeTableUniqueColumn(val, k)" multiple filterable clearable collapse-tags collapse-tags-tooltip class="w100">
											<el-option v-for="item in state.columnData" :key="item.propertyName" :label="item.propertyName + ' [' + item.columnComment + ']'" :value="item.propertyName" />
										</el-select>
									</el-form-item>
								</el-col>
								<el-col :xs="24" :sm="10" :md="10" :lg="10" :xl="10" class="mb20">
									<el-form-item label="描述信息" :prop="`tableUniqueList[${k}].message`" :rules="[{ required: true, message: `描述信息不能为空`, trigger: 'blur' }]">
										<el-input v-model="state.ruleForm.tableUniqueList[k].message" clearable placeholder="请输入" />
									</el-form-item>
								</el-col>
							</el-row>
						</template>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditCodeGen">
import { computed, onMounted, reactive, ref } from 'vue';
import IconSelector from '/@/components/iconSelector/index.vue';
import other from '/@/utils/other';
import { getAPI } from '/@/utils/axios-utils';
import { SysCodeGenApi, SysMenuApi, SysPrintApi } from '/@/api-services/api';
import { UpdateCodeGenInput, AddCodeGenInput, SysMenu, SysPrint } from '/@/api-services/models';

const props = defineProps({
	title: String,
	applicationNamespaces: Array<String>,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} as any,
	tableData: [] as any,
	dbData: [] as any,
	columnData: [] as any,
	menuData: [] as Array<SysMenu>,
	printList: [] as Array<SysPrint>,
});
// 级联选择器配置选项
const cascaderProps = { checkStrictly: true, emitPath: false, value: 'id', label: 'title' };

onMounted(async () => {
  state.dbData = await getAPI(SysCodeGenApi).apiSysCodeGenDatabaseListGet().then(res => res.data.result ?? []);
  state.printList = await getAPI(SysPrintApi).apiSysPrintPagePost().then(res => res.data.result?.items ?? []);
  state.menuData = await getAPI(SysMenuApi).apiSysMenuListGet().then(res => res.data.result ?? []);
});

// db改变
const dbChanged = async () => {
	if (state.ruleForm.configId === '') return;
    state.tableData = await getAPI(SysCodeGenApi).apiSysCodeGenTableListConfigIdGet(state.ruleForm.configId as string).then(res => res.data.result ?? []);

	let db = state.dbData.filter((u: any) => u.configId == state.ruleForm.configId);
	state.ruleForm.connectionString = db[0].connectionString;
	state.ruleForm.dbType = db[0].dbType.toString();
};

// table改变
const tableChanged = (item: any) => {
	state.ruleForm.tableName = item.tableName;
	state.ruleForm.entityName = item.entityName;
	state.ruleForm.busName = item.tableComment;
    state.ruleForm.tableUniqueList = [];
	getColumnInfoList();
};

// 表唯一约束配置项字段改变事件
const changeTableUniqueColumn = (value: any, index: number) => {
  if (value?.length === 1 && !state.ruleForm.tableUniqueList[index].message) {
    state.ruleForm.tableUniqueList[index].message = state.columnData.find((u: any) => u.propertyName === value[0])?.columnComment;
  }
}

const getColumnInfoList = async () => {
	if (state.ruleForm.configId == '' || state.ruleForm.tableName == '') return;
  state.columnData = await getAPI(SysCodeGenApi)
      .apiSysCodeGenColumnListByTableNameTableNameConfigIdGet(state.ruleForm.tableName, state.ruleForm.configId)
      .then(res => res.data.result)
      ?? [];
};

// 菜单改变
const menuChange = (menu: any) => {
	state.ruleForm.pagePath = state.menuData.find((x) => x.id == menu)?.name;
};

// print改变
const printTypeChanged = () => {
	if (state.ruleForm.printType === '') return;
	if (state.ruleForm.printType == 'off') state.ruleForm.printName = '';
};

// 获取全局组件大小
const getGlobalComponentSize = computed(() => {
	return other.globalComponentSize();
});

// 打开弹窗
const openDialog = (row: any) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
  dbChanged().then(() => getColumnInfoList());
  state.ruleForm.tableUniqueList ??= [];
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
    	if (state.ruleForm.tableUniqueList?.length === 0) state.ruleForm.tableUniqueList = null;
		state.ruleForm.tableName = state.ruleForm.entityName?? state.ruleForm.tableName;
		
		if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
			await getAPI(SysCodeGenApi).apiSysCodeGenUpdatePost(state.ruleForm as UpdateCodeGenInput);
		} else {
			await getAPI(SysCodeGenApi).apiSysCodeGenAddPost(state.ruleForm as AddCodeGenInput);
		}
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>

<style lang="scss" scoped>
:deep(.el-dialog__body) {
	min-height: 450px;
}
</style>
