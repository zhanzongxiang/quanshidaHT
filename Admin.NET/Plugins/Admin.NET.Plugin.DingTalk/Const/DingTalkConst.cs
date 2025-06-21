﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.DingTalk;

/// <summary>
/// 钉钉相关常量
/// </summary>
[Const("钉钉相关常量")]
public class DingTalkConst
{
    /// <summary>
    /// API分组名称
    /// </summary>
    public const string GroupName = "DingTalk";

    /// <summary>
    /// 姓名
    /// </summary>
    public const string NameField = "sys00-name";

    /// <summary>
    /// 手机号
    /// </summary>
    public const string MobileField = "sys00-mobile";

    /// <summary>
    /// 工号
    /// </summary>
    public const string JobNumberField = "sys00-jobNumber";

    /// <summary>
    /// 主部门Id
    /// </summary>
    public const string DeptId = "sys00-mainDeptId";

    /// <summary>
    /// 主部门
    /// </summary>
    public const string Dept = "sys00-mainDept";

    /// <summary>
    /// 职位
    /// </summary>
    public const string Position = "sys00-position";
}