// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 系统租户表
/// </summary>
[SugarTable(null, "系统租户表")]
[SysTable]
public partial class SysTenant : EntityBase
{
    /// <summary>
    /// 租管用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租管用户Id")]
    public virtual long UserId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public virtual long OrgId { get; set; }

    /// <summary>
    /// 域名
    /// </summary>
    [SugarColumn(ColumnDescription = "域名", Length = 128)]
    [MaxLength(128)]
    public virtual string? Host { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [SugarColumn(ColumnDescription = "租户类型")]
    public virtual TenantTypeEnum TenantType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型")]
    public virtual SqlSugar.DbType DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库连接", Length = 256)]
    [MaxLength(256)]
    public virtual string? Connection { get; set; }

    /// <summary>
    /// 数据库标识
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库标识", Length = 64)]
    [MaxLength(64)]
    public virtual string? ConfigId { get; set; }

    /// <summary>
    /// 从库连接/读写分离
    /// </summary>
    [SugarColumn(ColumnDescription = "从库连接/读写分离", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string? SlaveConnections { get; set; }

    /// <summary>
    /// 启用注册功能
    /// </summary>
    [SugarColumn(ColumnDescription = "启用注册功能")]
    public virtual YesNoEnum? EnableReg { get; set; } = YesNoEnum.N;

    /// <summary>
    /// 默认注册方案Id
    /// </summary>
    [SugarColumn(ColumnDescription = "默认注册方案")]
    public virtual long? RegWayId { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnDescription = "图标", Length = 256), MaxLength(256)]
    public virtual string? Logo { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(ColumnDescription = "标题", Length = 32), MaxLength(32)]
    public virtual string? Title { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    [SugarColumn(ColumnDescription = "副标题", Length = 32), MaxLength(32)]
    public virtual string? ViceTitle { get; set; }

    /// <summary>
    /// 副描述
    /// </summary>
    [SugarColumn(ColumnDescription = "副描述", Length = 64), MaxLength(64)]
    public virtual string? ViceDesc { get; set; }

    /// <summary>
    /// 水印
    /// </summary>
    [SugarColumn(ColumnDescription = "水印", Length = 32), MaxLength(32)]
    public virtual string? Watermark { get; set; }

    /// <summary>
    /// 版权信息
    /// </summary>
    [SugarColumn(ColumnDescription = "版权信息", Length = 64), MaxLength(64)]
    public virtual string? Copyright { get; set; }

    /// <summary>
    /// ICP备案号
    /// </summary>
    [SugarColumn(ColumnDescription = "ICP备案号", Length = 32), MaxLength(32)]
    public virtual string? Icp { get; set; }

    /// <summary>
    /// ICP地址
    /// </summary>
    [SugarColumn(ColumnDescription = "ICP地址", Length = 32), MaxLength(32)]
    public virtual string? IcpUrl { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public virtual int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 128)]
    [MaxLength(128)]
    public virtual string? Remark { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public virtual StatusEnum Status { get; set; } = StatusEnum.Enable;
}