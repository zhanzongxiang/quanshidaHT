﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 系统作业触发器表
/// </summary>
[SugarTable(null, "系统作业触发器表")]
[SysTable]
public partial class SysJobTrigger : EntityBaseId
{
    /// <summary>
    /// 触发器Id
    /// </summary>
    [SugarColumn(ColumnDescription = "触发器Id", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string TriggerId { get; set; }

    /// <summary>
    /// 作业Id
    /// </summary>
    [SugarColumn(ColumnDescription = "作业Id", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string JobId { get; set; }

    /// <summary>
    /// 触发器类型FullName
    /// </summary>
    [SugarColumn(ColumnDescription = "触发器类型", Length = 128)]
    [MaxLength(128)]
    public string? TriggerType { get; set; }

    /// <summary>
    /// 程序集Name
    /// </summary>
    [SugarColumn(ColumnDescription = "程序集", Length = 128)]
    [MaxLength(128)]
    public string? AssemblyName { get; set; } = "Furion.Pure";

    /// <summary>
    /// 参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", Length = 128)]
    [MaxLength(128)]
    public string? Args { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnDescription = "描述信息", Length = 128)]
    [MaxLength(128)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public TriggerStatus Status { get; set; } = TriggerStatus.Ready;

    /// <summary>
    /// 起始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "起始时间")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近运行时间")]
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    [SugarColumn(ColumnDescription = "下一次运行时间")]
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    [SugarColumn(ColumnDescription = "触发次数")]
    public long NumberOfRuns { get; set; }

    /// <summary>
    /// 最大触发次数（0:不限制，n:N次）
    /// </summary>
    [SugarColumn(ColumnDescription = "最大触发次数")]
    public long MaxNumberOfRuns { get; set; }

    /// <summary>
    /// 出错次数
    /// </summary>
    [SugarColumn(ColumnDescription = "出错次数")]
    public long NumberOfErrors { get; set; }

    /// <summary>
    /// 最大出错次数（0:不限制，n:N次）
    /// </summary>
    [SugarColumn(ColumnDescription = "最大出错次数")]
    public long MaxNumberOfErrors { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    [SugarColumn(ColumnDescription = "重试次数")]
    public int NumRetries { get; set; }

    /// <summary>
    /// 重试间隔时间（ms）
    /// </summary>
    [SugarColumn(ColumnDescription = "重试间隔时间(ms)")]
    public int RetryTimeout { get; set; } = 1000;

    /// <summary>
    /// 是否立即启动
    /// </summary>
    [SugarColumn(ColumnDescription = "是否立即启动")]
    public bool StartNow { get; set; } = true;

    /// <summary>
    /// 是否启动时执行一次
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启动时执行一次")]
    public bool RunOnStart { get; set; } = false;

    /// <summary>
    /// 是否在启动时重置最大触发次数等于一次的作业
    /// </summary>
    [SugarColumn(ColumnDescription = "是否重置触发次数")]
    public bool ResetOnlyOnce { get; set; } = true;

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public DateTime? UpdatedTime { get; set; }
}