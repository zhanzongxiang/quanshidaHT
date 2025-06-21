// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.IO.Compression;

namespace Admin.NET.Core.Service;

/// <summary>
/// 系统代码生成器服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 270)]
public class SysCodeGenService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;

    private readonly SysCodeGenConfigService _codeGenConfigService;
    private readonly DbConnectionOptions _dbConnectionOptions;
    private readonly CodeGenOptions _codeGenOptions;
    private readonly SysMenuService _sysMenuService;
    private readonly IViewEngine _viewEngine;
    private readonly UserManager _userManager;

    public SysCodeGenService(ISqlSugarClient db,
        IOptions<DbConnectionOptions> dbConnectionOptions,
        SysCodeGenConfigService codeGenConfigService,
        IOptions<CodeGenOptions> codeGenOptions,
        SysMenuService sysMenuService,
        UserManager userManager,
        IViewEngine viewEngine)
    {
        _db = db;
        _viewEngine = viewEngine;
        _userManager = userManager;
        _sysMenuService = sysMenuService;
        _codeGenOptions = codeGenOptions.Value;
        _codeGenConfigService = codeGenConfigService;
        _dbConnectionOptions = dbConnectionOptions.Value;
    }

    /// <summary>
    /// 获取代码生成分页列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取代码生成分页列表")]
    public async Task<SqlSugarPagedList<SysCodeGen>> Page(CodeGenInput input)
    {
        return await _db.Queryable<SysCodeGen>()
            .WhereIF(!string.IsNullOrWhiteSpace(input.TableName), u => u.TableName.Contains(input.TableName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BusName), u => u.BusName.Contains(input.BusName.Trim()))
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加代码生成 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加代码生成")]
    public async Task AddCodeGen(AddCodeGenInput input)
    {
        var isExist = await _db.Queryable<SysCodeGen>().Where(u => u.TableName == input.TableName).AnyAsync();
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1400);

        if (input.TableUniqueList?.Count > 0) input.TableUniqueConfig = JSON.Serialize(input.TableUniqueList);

        var codeGen = input.Adapt<SysCodeGen>();
        var newCodeGen = await _db.Insertable(codeGen).ExecuteReturnEntityAsync();

        // 增加配置表
        _codeGenConfigService.AddList(GetColumnList(input), newCodeGen);
    }

    /// <summary>
    /// 更新代码生成 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新代码生成")]
    public async Task UpdateCodeGen(UpdateCodeGenInput input)
    {
        var isExist = await _db.Queryable<SysCodeGen>().AnyAsync(u => u.TableName == input.TableName && u.Id != input.Id);
        if (isExist) throw Oops.Oh(ErrorCodeEnum.D1400);

        var oldRecord = await _db.Queryable<SysCodeGen>().FirstAsync(u => u.Id == input.Id);

        if (input.TableUniqueList?.Count > 0) input.TableUniqueConfig = JSON.Serialize(input.TableUniqueList);
        var codeGen = input.Adapt<SysCodeGen>();
        await _db.Updateable(codeGen).ExecuteCommandAsync();

        // 仅当数据表名称发生了变化，才更新配置表
        //if (oldRecord.TableName != input.TableName)
        //{
            await _codeGenConfigService.DeleteCodeGenConfig(codeGen.Id);
            _codeGenConfigService.AddList(GetColumnList(input.Adapt<AddCodeGenInput>()), codeGen);
        //}
    }

    /// <summary>
    /// 删除代码生成 🔖
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除代码生成")]
    public async Task DeleteCodeGen(List<DeleteCodeGenInput> inputs)
    {
        if (inputs == null || inputs.Count < 1) return;

        var codeGenConfigTaskList = new List<Task>();
        inputs.ForEach(u =>
        {
            _db.Deleteable<SysCodeGen>().In(u.Id).ExecuteCommand();

            // 删除配置表
            codeGenConfigTaskList.Add(_codeGenConfigService.DeleteCodeGenConfig(u.Id));
        });
        await Task.WhenAll(codeGenConfigTaskList);
    }

    /// <summary>
    /// 获取代码生成详情 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取代码生成详情")]
    public async Task<SysCodeGen> GetDetail([FromQuery] QueryCodeGenInput input)
    {
        return await _db.Queryable<SysCodeGen>().SingleAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取数据库库集合 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取数据库库集合")]
    public async Task<List<DatabaseOutput>> GetDatabaseList()
    {
        var dbConfigs = _dbConnectionOptions.ConnectionConfigs;
        return await Task.FromResult(dbConfigs.Adapt<List<DatabaseOutput>>());
    }

    /// <summary>
    /// 获取数据库表(实体)集合 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取数据库表(实体)集合")]
    public async Task<List<TableOutput>> GetTableList(string configId = SqlSugarConst.MainConfigId)
    {
        var provider = _db.AsTenant().GetConnectionScope(configId);
        var dbTableInfos = provider.DbMaintenance.GetTableInfoList(false); // 不能走缓存,否则切库不起作用
        var config = _dbConnectionOptions.ConnectionConfigs.FirstOrDefault(u => configId.Equals(u.ConfigId));

        // var dbTableNames = dbTableInfos.Select(u => u.Name.ToLower()).ToList();
        IEnumerable<EntityInfo> entityInfos = await GetEntityInfos(configId);

        var tableOutputList = new List<TableOutput>();
        foreach (var item in entityInfos)
        {
            var tbConfigId = item.Type.GetCustomAttribute<TenantAttribute>()?.configId as string ?? SqlSugarConst.MainConfigId;
            if (item.Type.IsDefined(typeof(LogTableAttribute))) tbConfigId = SqlSugarConst.LogConfigId;
            if (tbConfigId != configId) continue;

            var table = dbTableInfos.FirstOrDefault(u => string.Equals(u.Name, (config!.DbSettings.EnableUnderLine ? UtilMethods.ToUnderLine(item.DbTableName) : item.DbTableName), StringComparison.CurrentCultureIgnoreCase));
            if (table == null) continue;
            tableOutputList.Add(new TableOutput
            {
                ConfigId = configId,
                EntityName = item.EntityName,
                TableName = table.Name,
                TableComment = item.TableDescription
            });
        }
        return tableOutputList;
    }

    /// <summary>
    /// 根据表名获取列集合 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("根据表名获取列集合")]
    public List<ColumnOuput> GetColumnListByTableName([Required] string tableName, string configId = SqlSugarConst.MainConfigId)
    {
        // 切库---多库代码生成用
        var provider = _db.AsTenant().GetConnectionScope(configId);
        var config = _dbConnectionOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == configId) ?? throw Oops.Oh(ErrorCodeEnum.D1401);
        if (config.DbSettings.EnableUnderLine) tableName = UtilMethods.ToUnderLine(tableName);
        // 获取实体类型属性
        var entityType = provider.DbMaintenance.GetTableInfoList(false).FirstOrDefault(u => u.Name == tableName);
        if (entityType == null) return null;
        var properties = GetEntityInfos(configId).Result.First(e => e.DbTableName.EndsWithIgnoreCase(tableName)).Type.GetProperties()
            .Where(e => e.GetCustomAttribute<SugarColumn>()?.IsIgnore == false).Select(e => new
            {
                PropertyName = e.Name,
                ColumnComment = e.GetCustomAttribute<SugarColumn>()?.ColumnDescription,
                ColumnName = e.GetCustomAttribute<SugarColumn>()?.ColumnName ?? e.Name
            }).ToList();
        // 按原始类型的顺序获取所有实体类型属性（不包含导航属性，会返回null）
        var columnList = provider.DbMaintenance.GetColumnInfosByTableName(tableName).Select(u => new ColumnOuput
        {
            ColumnName = config!.DbSettings.EnableUnderLine ? UtilMethods.ToUnderLine(u.DbColumnName) : u.DbColumnName,
            ColumnKey = u.IsPrimarykey.ToString(),
            DataType = u.DataType.ToString(),
            NetType = CodeGenUtil.ConvertDataType(u, provider.CurrentConnectionConfig.DbType),
            ColumnComment = u.ColumnDescription
        }).ToList();
        foreach (var column in columnList)
        {
            var property = properties.FirstOrDefault(e => (config!.DbSettings.EnableUnderLine ? UtilMethods.ToUnderLine(e.ColumnName) : e.ColumnName) == column.ColumnName);
            column.ColumnComment ??= property?.ColumnComment;
            column.PropertyName = property?.PropertyName;
        }
        return columnList;
    }

    /// <summary>
    /// 获取数据表列（实体属性）集合
    /// </summary>
    /// <returns></returns>
    private List<ColumnOuput> GetColumnList([FromQuery] AddCodeGenInput input)
    {
        var entityType = GetEntityInfos(input.ConfigId).GetAwaiter().GetResult().FirstOrDefault(u => u.EntityName == input.TableName);
        if (entityType == null) return null;

        var config = _dbConnectionOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == input.ConfigId);
        var dbTableName = config!.DbSettings.EnableUnderLine ? UtilMethods.ToUnderLine(entityType.DbTableName) : entityType.DbTableName;

        // 切库---多库代码生成用
        var provider = _db.AsTenant().GetConnectionScope(!string.IsNullOrEmpty(input.ConfigId) ? input.ConfigId : SqlSugarConst.MainConfigId);

        var entityBasePropertyNames = CodeGenUtil.GetPropertyInfoArray(typeof(EntityBaseTenant))?.Select(p => p.Name).ToArray();
        var columnInfos = provider.DbMaintenance.GetColumnInfosByTableName(dbTableName, false);
        var result = columnInfos.Select(u => new ColumnOuput
        {
            // 转下划线后的列名需要再转回来（暂时不转）
            //ColumnName = config.DbSettings.EnableUnderLine ? CodeGenUtil.CamelColumnName(u.DbColumnName, entityBasePropertyNames) : u.DbColumnName,
            ColumnName = u.DbColumnName,
            ColumnLength = u.Length,
            IsPrimarykey = u.IsPrimarykey,
            IsNullable = u.IsNullable,
            ColumnKey = u.IsPrimarykey.ToString(),
            NetType = CodeGenUtil.ConvertDataType(u, provider.CurrentConnectionConfig.DbType),
            DataType = CodeGenUtil.ConvertDataType(u, provider.CurrentConnectionConfig.DbType),
            ColumnComment = string.IsNullOrWhiteSpace(u.ColumnDescription) ? u.DbColumnName : u.ColumnDescription,
            DefaultValue = u.DefaultValue,
        }).ToList();

        // 获取实体的属性信息，赋值给PropertyName属性(CodeFirst模式应以PropertyName为实际使用名称)
        var entityProperties = entityType.Type.GetProperties();

        for (int i = result.Count - 1; i >= 0; i--)
        {
            var columnOutput = result[i];
            // 先找自定义字段名的，如果找不到就再找自动生成字段名的(并且过滤掉没有SugarColumn的属性)
            var propertyInfo = entityProperties.FirstOrDefault(u => string.Equals((u.GetCustomAttribute<SugarColumn>()?.ColumnName ?? ""), columnOutput.ColumnName, StringComparison.CurrentCultureIgnoreCase)) ??
                entityProperties.FirstOrDefault(u => u.GetCustomAttribute<SugarColumn>() != null && u.Name.ToLower() == (config.DbSettings.EnableUnderLine
                ? CodeGenUtil.CamelColumnName(columnOutput.ColumnName, entityBasePropertyNames).ToLower()
                : columnOutput.ColumnName.ToLower()));
            if (propertyInfo != null)
            {
                columnOutput.PropertyName = propertyInfo.Name;
                columnOutput.ColumnComment = propertyInfo.GetCustomAttribute<SugarColumn>()!.ColumnDescription;
                var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                if (propertyInfo.PropertyType.IsEnum || (propertyType?.IsEnum ?? false))
                {
                    columnOutput.DictTypeCode = (propertyType ?? propertyInfo.PropertyType).Name;
                }
                else
                {
                    var dict = propertyInfo.GetCustomAttribute<DictAttribute>();
                    if (dict != null) columnOutput.DictTypeCode = dict.DictTypeCode;
                }
            }
            else
            {
                result.RemoveAt(i); // 移除没有定义此属性的字段
            }
        }
        return result;
    }

    /// <summary>
    /// 获取库表信息
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<EntityInfo>> GetEntityInfos(string configId)
    {
        var config = _dbConnectionOptions.ConnectionConfigs.FirstOrDefault(u => u.ConfigId.ToString() == configId) ?? throw Oops.Oh(ErrorCodeEnum.D1401);
        var entityInfos = new List<EntityInfo>();

        var type = typeof(SugarTable);
        var types = new List<Type>();
        if (_codeGenOptions.EntityAssemblyNames != null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name;
                if (!_codeGenOptions.EntityAssemblyNames.Contains(assemblyName) && !_codeGenOptions.EntityAssemblyNames.Any(name => assemblyName!.Contains(name))) continue;

                Assembly asm = Assembly.Load(assemblyName!);
                types.AddRange(asm.GetExportedTypes().ToList());
            }
        }

        Type[] cosType = types.Where(o => IsMyAttribute(Attribute.GetCustomAttributes(o, true))).ToArray();

        foreach (var ct in cosType)
        {
            var sugarAttribute = ct.GetCustomAttributes(type, true).FirstOrDefault();

            var description = "";
            var des = ct.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (des.Length > 0) description = ((DescriptionAttribute)des[0]).Description;

            var dbTableName = sugarAttribute == null || string.IsNullOrWhiteSpace(((SugarTable)sugarAttribute).TableName) ? ct.Name : ((SugarTable)sugarAttribute).TableName;
            if (config.DbSettings.EnableUnderLine) dbTableName = UtilMethods.ToUnderLine(dbTableName);

            entityInfos.Add(new EntityInfo
            {
                EntityName = ct.Name,
                DbTableName = dbTableName,
                TableDescription = sugarAttribute == null ? description : ((SugarTable)sugarAttribute).TableDescription,
                Type = ct
            });
        }
        return await Task.FromResult(entityInfos);

        bool IsMyAttribute(Attribute[] o) => o.Any(a => a.GetType() == type);
    }

    /// <summary>
    /// 获取程序保存位置 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取程序保存位置")]
    public List<string> GetApplicationNamespaces()
    {
        return _codeGenOptions.BackendApplicationNamespaces;
    }

    /// <summary>
    /// 代码生成到本地 🔖
    /// </summary>
    /// <returns></returns>
    [UnitOfWork]
    [DisplayName("代码生成到本地")]
    public async Task<dynamic> RunLocal(SysCodeGen input)
    {
        if (string.IsNullOrEmpty(input.GenerateType))
            input.GenerateType = "200";

        // 先删除该表已生成的菜单列表
        List<string> targetPathList;
        var zipPath = Path.Combine(App.WebHostEnvironment.WebRootPath, "CodeGen", input.TableName!);
        if (input.GenerateType.StartsWith('1'))
        {
            targetPathList = GetZipPathList(input);
            if (Directory.Exists(zipPath)) Directory.Delete(zipPath, true);
        }
        else
            targetPathList = GetTargetPathList(input);

        var (tableFieldList, result) = await RenderTemplateAsync(input);
        var templatePathList = GetTemplatePathList(input);
        for (var i = 0; i < templatePathList.Count; i++)
        {
            var content = result.GetValueOrDefault(templatePathList[i]?.TrimEnd(".vm"));
            if (string.IsNullOrWhiteSpace(content)) continue;
            var dirPath = new DirectoryInfo(targetPathList[i]).Parent!.FullName;
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            _ = File.WriteAllTextAsync(targetPathList[i], content, Encoding.UTF8);
        }

        if (input.GenerateMenu) await AddMenu(input.TableName, input.BusName, input.MenuPid ?? 0, input.MenuIcon, input.PagePath, tableFieldList);

        // 非ZIP压缩返回空
        if (!input.GenerateType.StartsWith('1')) return null;

        // 判断是否存在同名称文件
        string downloadPath = zipPath + ".zip";
        if (File.Exists(downloadPath)) File.Delete(downloadPath);

        // 创建zip文件并返回下载地址
        ZipFile.CreateFromDirectory(zipPath, downloadPath);
        return new { url = $"{App.HttpContext.Request.Scheme}://{App.HttpContext.Request.Host.Value}/codeGen/{input.TableName}.zip" };
    }

    /// <summary>
    /// 获取代码生成预览 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取代码生成预览")]
    public async Task<Dictionary<string, string>> Preview(SysCodeGen input)
    {
        var (_, result) = await RenderTemplateAsync(input);
        return result;
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task<(List<CodeGenConfig> tableFieldList, Dictionary<string, string> result)> RenderTemplateAsync(SysCodeGen input)
    {
        var tableFieldList = await _codeGenConfigService.GetList(new CodeGenConfig { CodeGenId = input.Id }); // 字段集合
        var joinTableList = tableFieldList.Where(u => u.EffectType is "Upload" or "ForeignKey" or "ApiTreeSelector").ToList(); // 需要连表查询的字段

        var data = new CustomViewEngine
        {
            ConfigId = input.ConfigId,
            BusName = input.BusName,
            PagePath = input.PagePath,
            NameSpace = input.NameSpace,
            ClassName = input.TableName,
            PrintType = input.PrintType,
            PrintName = input.PrintName,
            AuthorName = input.AuthorName,
            ProjectLastName = input.NameSpace!.Split('.').Last(),
            LowerClassName = input.TableName!.ToFirstLetterLowerCase(),
            TableUniqueConfigList = input.TableUniqueList ?? new(),

            TableField = tableFieldList,
            QueryWhetherList = tableFieldList.Where(u => u.WhetherQuery == "Y").ToList(),
            ImportFieldList = tableFieldList.Where(u => u.WhetherImport == "Y").ToList(),
            UploadFieldList = tableFieldList.Where(u => u.EffectType == "Upload").ToList(),
            PrimaryKeyFieldList = tableFieldList.Where(c => c.ColumnKey == "True").ToList(),
            AddUpdateFieldList = tableFieldList.Where(u => u.WhetherAddUpdate == "Y").ToList(),
            ApiTreeFieldList = tableFieldList.Where(u => u.EffectType == "ApiTreeSelector").ToList(),
            DropdownFieldList = tableFieldList.Where(u => u.EffectType is "ForeignKey" or "ApiTreeSelector").ToList(),
            DefaultValueList = tableFieldList.Where(u => u.DefaultValue!=null && u.DefaultValue.Length>0).ToList(),

            HasJoinTable = joinTableList.Count > 0,
            HasDictField = tableFieldList.Any(u => u.EffectType == "DictSelector"),
            HasEnumField = tableFieldList.Any(u => u.EffectType == "EnumSelector"),
            HasConstField = tableFieldList.Any(u => u.EffectType == "ConstSelector"),
            HasLikeQuery = tableFieldList.Any(c => c.WhetherQuery == "Y" && c.QueryType == "like")
        };

        // 获取模板文件并替换
        var templatePathList = GetTemplatePathList();
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "template");

        var result = new Dictionary<string, string>();
        foreach (var path in templatePathList)
        {
            var templateFilePath = Path.Combine(templatePath, path);
            if (!File.Exists(templateFilePath)) continue;
            var tContent = await File.ReadAllTextAsync(templateFilePath);
            var tResult = await _viewEngine.RunCompileFromCachedAsync(tContent, data, builderAction: builder =>
            {
                builder.AddAssemblyReferenceByName("System.Text.RegularExpressions");
                builder.AddAssemblyReferenceByName("System.Collections");
                builder.AddAssemblyReferenceByName("System.Linq");

                builder.AddUsing("System.Text.RegularExpressions");
                builder.AddUsing("System.Collections.Generic");
                builder.AddUsing("System.Linq");
            });
            result.Add(path?.TrimEnd(".vm"), tResult);
        }
        return (tableFieldList, result);
    }

    /// <summary>
    /// 增加菜单
    /// </summary>
    /// <param name="className"></param>
    /// <param name="busName"></param>
    /// <param name="pid"></param>
    /// <param name="menuIcon"></param>
    /// <param name="pagePath"></param>
    /// <param name="tableFieldList"></param>
    /// <returns></returns>
    private async Task AddMenu(string className, string busName, long pid, string menuIcon, string pagePath, List<CodeGenConfig> tableFieldList)
    {
        // 删除已存在的菜单
        var title = $"{busName}管理";
        await DeleteMenuTree(title, pid == 0 ? MenuTypeEnum.Dir : MenuTypeEnum.Menu);

        var parentMenuPath = "";
        var lowerClassName = className!.ToFirstLetterLowerCase();
        if (pid == 0)
        {
            // 新增目录，并记录Id
            var dirMenu = new SysMenu { Pid = 0, Title = title, Type = MenuTypeEnum.Dir, Icon = "robot", Path = "/" + className.ToLower(), Component = "Layout" };
            pid = await _sysMenuService.AddMenu(dirMenu.Adapt<AddMenuInput>());
        }
        else
        {
            var parentMenu = await _db.Queryable<SysMenu>().FirstAsync(u => u.Id == pid) ?? throw Oops.Oh(ErrorCodeEnum.D1505);
            parentMenuPath = parentMenu.Path;
        }

        // 新增菜单，并记录Id
        var rootMenu = new SysMenu { Pid = pid, Title = title, Type = MenuTypeEnum.Menu, Icon = menuIcon, Path = $"{parentMenuPath}/{className.ToLower()}", Component = $"/{pagePath}/{lowerClassName}/index" };
        pid = await _sysMenuService.AddMenu(rootMenu.Adapt<AddMenuInput>());

        var orderNo = 100;
        var menuList = new List<SysMenu>
        {
            new() { Title="查询", Permission=$"{lowerClassName}:page", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="详情", Permission=$"{lowerClassName}:detail", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="增加", Permission=$"{lowerClassName}:add", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="编辑", Permission=$"{lowerClassName}:update", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="删除", Permission=$"{lowerClassName}:delete", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="批量删除", Permission=$"{lowerClassName}:batchDelete", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="设置状态", Permission=$"{lowerClassName}:setStatus", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="打印", Permission=$"{lowerClassName}:print", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="导入", Permission=$"{lowerClassName}:import", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10},
            new() { Title="导出", Permission=$"{lowerClassName}:export", Pid=pid, Type=MenuTypeEnum.Btn, OrderNo=orderNo+=10}
        };

        if (tableFieldList.Any(u => u.EffectType is "ForeignKey" or "ApiTreeSelector" && (u.WhetherAddUpdate == "Y" || u.WhetherQuery == "Y")))
            menuList.Add(new SysMenu { Title = "下拉列表数据", Permission = $"{lowerClassName}:dropdownData", Pid = pid, Type = MenuTypeEnum.Btn, OrderNo = orderNo += 10 });

        foreach (var column in tableFieldList.Where(u => u.EffectType == "Upload"))
            menuList.Add(new SysMenu { Title = $"上传{column.ColumnComment}", Permission = $"{lowerClassName}:upload{column.PropertyName}", Pid = pid, Type = MenuTypeEnum.Btn, OrderNo = orderNo += 10 });

        foreach (var menu in menuList) await _sysMenuService.AddMenu(menu.Adapt<AddMenuInput>());
    }

    /// <summary>
    /// 根据菜单名称和类型删除关联的菜单树
    /// </summary>
    /// <param name="title"></param>
    /// <param name="type"></param>
    private async Task DeleteMenuTree(string title, MenuTypeEnum type)
    {
        var menuList = await _db.Queryable<SysMenu>().Where(u => u.Title == title && u.Type == type).ToListAsync() ?? new();
        foreach (var menu in menuList) await _sysMenuService.DeleteMenu(new DeleteMenuInput { Id = menu.Id });
    }

    /// <summary>
    /// 获取模板文件路径集合
    /// </summary>
    /// <returns></returns>
    private static List<string> GetTemplatePathList(SysCodeGen input)
    {
        if (input.GenerateType!.Substring(1, 1).Contains('1')) return new() { "index.vue.vm", "editDialog.vue.vm", "api.ts.vm" };
        if (input.GenerateType.Substring(1, 1).Contains('2')) return new() { "Service.cs.vm", "Input.cs.vm", "Output.cs.vm", "Dto.cs.vm" };
        return new() { "Service.cs.vm", "Input.cs.vm", "Output.cs.vm", "Dto.cs.vm", "index.vue.vm", "editDialog.vue.vm", "api.ts.vm" };
    }

    /// <summary>
    /// 获取模板文件路径集合
    /// </summary>
    /// <returns></returns>
    private static List<string> GetTemplatePathList() => new() { "Service.cs.vm", "Input.cs.vm", "Output.cs.vm", "Dto.cs.vm", "index.vue.vm", "editDialog.vue.vm", "api.ts.vm" };

    /// <summary>
    /// 设置生成文件路径
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private List<string> GetTargetPathList(SysCodeGen input)
    {
        //var backendPath = Path.Combine(new DirectoryInfo(App.WebHostEnvironment.ContentRootPath).Parent.FullName, _codeGenOptions.BackendApplicationNamespace, "Service", input.TableName);
        var backendPath = Path.Combine(new DirectoryInfo(App.WebHostEnvironment.ContentRootPath).Parent!.FullName, input.NameSpace!, "Service", input.TableName!);
        var servicePath = Path.Combine(backendPath, input.TableName + "Service.cs");
        var inputPath = Path.Combine(backendPath, "Dto", input.TableName + "Input.cs");
        var outputPath = Path.Combine(backendPath, "Dto", input.TableName + "Output.cs");
        var viewPath = Path.Combine(backendPath, "Dto", input.TableName + "Dto.cs");
        var frontendPath = Path.Combine(new DirectoryInfo(App.WebHostEnvironment.ContentRootPath).Parent!.Parent!.FullName, _codeGenOptions.FrontRootPath, "src", "views", input.PagePath!);
        var indexPath = Path.Combine(frontendPath, input.TableName[..1].ToLower() + input.TableName[1..], "index.vue");//
        var formModalPath = Path.Combine(frontendPath, input.TableName[..1].ToLower() + input.TableName[1..], "component", "editDialog.vue");
        var apiJsPath = Path.Combine(new DirectoryInfo(App.WebHostEnvironment.ContentRootPath).Parent!.Parent!.FullName, _codeGenOptions.FrontRootPath, "src", "api", input.PagePath, input.TableName[..1].ToLower() + input.TableName[1..] + ".ts");

        if (input.GenerateType!.Substring(1, 1).Contains('1'))
        {
            // 生成到本项目(前端)
            return new List<string>
            {
                indexPath,
                formModalPath,
                apiJsPath
            };
        }

        if (input.GenerateType.Substring(1, 1).Contains('2'))
        {
            // 生成到本项目(后端)
            return new List<string>
            {
                servicePath,
                inputPath,
                outputPath,
                viewPath,
            };
        }
        // 前后端同时生成到本项目
        return new List<string>
        {
            servicePath,
            inputPath,
            outputPath,
            viewPath,
            indexPath,
            formModalPath,
            apiJsPath
        };
    }

    /// <summary>
    /// 设置生成文件路径
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private List<string> GetZipPathList(SysCodeGen input)
    {
        var zipPath = Path.Combine(App.WebHostEnvironment.WebRootPath, "CodeGen", input.TableName!);

        var firstLowerTableName = input.TableName!.ToFirstLetterLowerCase();
        //var backendPath = Path.Combine(zipPath, _codeGenOptions.BackendApplicationNamespace, "Service", input.TableName);
        var backendPath = Path.Combine(zipPath, input.NameSpace!, "Service", input.TableName);
        var servicePath = Path.Combine(backendPath, input.TableName + "Service.cs");
        var inputPath = Path.Combine(backendPath, "Dto", input.TableName + "Input.cs");
        var outputPath = Path.Combine(backendPath, "Dto", input.TableName + "Output.cs");
        var viewPath = Path.Combine(backendPath, "Dto", input.TableName + "Dto.cs");
        var frontendPath = Path.Combine(zipPath, _codeGenOptions.FrontRootPath, "src", "views", input.PagePath!);
        var indexPath = Path.Combine(frontendPath, firstLowerTableName, "index.vue");
        var formModalPath = Path.Combine(frontendPath, firstLowerTableName, "component", "editDialog.vue");
        var apiJsPath = Path.Combine(zipPath, _codeGenOptions.FrontRootPath, "src", "api", input.PagePath, firstLowerTableName + ".ts");
        if (input.GenerateType!.StartsWith("11"))
        {
            return new List<string>
            {
                indexPath,
                formModalPath,
                apiJsPath
            };
        }

        if (input.GenerateType.StartsWith("12"))
        {
            return new List<string>
            {
                servicePath,
                inputPath,
                outputPath,
                viewPath
            };
        }

        return new List<string>
        {
            servicePath,
            inputPath,
            outputPath,
            viewPath,
            indexPath,
            formModalPath,
            apiJsPath
        };
    }
}