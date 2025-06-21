﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 代码生成详细配置参数
/// </summary>
public class CodeGenConfig
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 代码生成主表ID
    /// </summary>
    public long CodeGenId { get; set; }

    /// <summary>
    /// 数据库字段名
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// 主外键
    /// </summary>
    public string ColumnKey { get; set; }

    /// <summary>
    /// 实体属性名
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// 字段数据长度
    /// </summary>
    public int ColumnLength { get; set; }

    /// <summary>
    /// 数据库字段名(首字母小写)
    /// </summary>
    public string LowerPropertyName => string.IsNullOrWhiteSpace(PropertyName) ? null : PropertyName[..1].ToLower() + PropertyName[1..];

    /// <summary>
    /// 字段描述
    /// </summary>
    public string ColumnComment { get; set; }

    /// <summary>
    /// .NET类型
    /// </summary>
    public string NetType { get; set; }

    /// <summary>
    /// 数据库中类型（物理类型）
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 字段数据默认值
    /// </summary>
    public string DefaultValue { get; set; }
    /// <summary>
    /// 可空.NET类型
    /// </summary>
    public string NullableNetType => Regex.IsMatch(NetType ?? "", "(.*?Enum|bool|char|int|long|double|float|decimal)[?]?") ? NetType.TrimEnd('?') + "?" : NetType;

    /// <summary>
    /// 作用类型（字典）
    /// </summary>
    public string EffectType { get; set; }

    /// <summary>
    /// 外键库标识
    /// </summary>
    public string FkConfigId { get; set; }

    /// <summary>
    /// 外键实体名称
    /// </summary>
    public string FkEntityName { get; set; }

    /// <summary>
    /// 外键表名称
    /// </summary>
    public string FkTableName { get; set; }

    /// <summary>
    /// 外键实体名称(首字母小写)
    /// </summary>
    public string LowerFkEntityName => string.IsNullOrWhiteSpace(FkEntityName) ? null : FkEntityName[..1].ToLower() + FkEntityName[1..];

    /// <summary>
    /// 外键链接字段
    /// </summary>
    public string FkLinkColumnName { get; set; }

    /// <summary>
    /// 外键显示字段
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string FkDisplayColumns { get; set; }

    /// <summary>
    /// 外键显示字段
    /// </summary>
    public List<string> FkDisplayColumnList { get; set; }

    /// <summary>
    /// 外键显示字段(首字母小写)
    /// </summary>
    public List<string> LowerFkDisplayColumnsList => FkDisplayColumnList?.Select(name => name[..1].ToLower() + name[1..]).ToList();

    /// <summary>
    /// 外键显示字段.NET类型
    /// </summary>
    public string FkColumnNetType { get; set; }

    /// <summary>
    /// 父级字段
    /// </summary>
    public string PidColumn { get; set; }

    /// <summary>
    /// 字典code
    /// </summary>
    public string DictTypeCode { get; set; }

    /// <summary>
    /// 查询方式
    /// </summary>
    public string QueryType { get; set; }

    /// <summary>
    /// 是否是查询条件
    /// </summary>
    public string WhetherQuery { get; set; }

    /// <summary>
    /// 列表是否缩进（字典）
    /// </summary>
    public string WhetherRetract { get; set; }

    /// <summary>
    /// 是否必填（字典）
    /// </summary>
    public string WhetherRequired { get; set; }

    /// <summary>
    /// 是否可排序（字典）
    /// </summary>
    public string WhetherSortable { get; set; }

    /// <summary>
    /// 列表显示
    /// </summary>
    public string WhetherTable { get; set; }

    /// <summary>
    /// 增改
    /// </summary>
    public string WhetherAddUpdate { get; set; }

    /// <summary>
    /// 导入
    /// </summary>
    public string WhetherImport { get; set; }

    /// <summary>
    /// 是否是通用字段
    /// </summary>
    public string WhetherCommon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 是否是选择器控件
    /// </summary>
    public bool IsSelectorEffectType => Regex.IsMatch(EffectType ?? "", "Selector$|ForeignKey", RegexOptions.IgnoreCase);

    /// <summary>
    /// 去掉尾部Id的属性名
    /// </summary>
    public string PropertyNameTrimEndId => PropertyName.TrimEnd("Id");

    /// <summary>
    /// 去掉尾部Id的属性名
    /// </summary>
    public string LowerPropertyNameTrimEndId => LowerPropertyName.TrimEnd("Id");

    /// <summary>
    /// 扩展属性名称
    /// </summary>
    public string ExtendedPropertyName => EffectType switch
    {
        "ForeignKey" => $"{PropertyName.TrimEnd("Id")}FkDisplayName",
        "ApiTreeSelector" => $"{PropertyName.TrimEnd("Id")}DisplayName",
        "DictSelector" => $"{PropertyName.TrimEnd("Id")}DictLabel",
        "Upload" => $"{PropertyName.TrimEnd("Id")}Attachment",
        _ => PropertyName
    };

    /// <summary>
    /// 首字母小写的扩展属性名称
    /// </summary>
    public string LowerExtendedPropertyName
    {
        get
        {
            var displayPropertyName = ExtendedPropertyName;
            if (string.IsNullOrWhiteSpace(displayPropertyName)) return null;
            return displayPropertyName[..1].ToLower() + displayPropertyName[1..];
        }
    }

    /// <summary>
    /// 获取外键显示值语句
    /// </summary>
    /// <param name="tableAlias">表别名</param>
    /// <param name="separator">多字段时的连接符</param>
    /// <returns></returns>
    public string GetDisplayColumn(string tableAlias, string separator = "-") => "$\"" + string.Join(separator, FkDisplayColumnList?.Select(name => $"{{{tableAlias}.{name}}}") ?? new List<string>()) + "\"";
}