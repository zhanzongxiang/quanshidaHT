﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 短信配置选项
/// </summary>
public sealed class SMSOptions : IConfigurableOptions
{
    /// <summary>
    /// Aliyun
    /// </summary>
    public SMSSettings Aliyun { get; set; }

    /// <summary>
    /// Tencentyun
    /// </summary>
    public SMSSettings Tencentyun { get; set; }
}

public sealed class SMSSettings
{
    /// <summary>
    /// SdkAppId
    /// </summary>
    public string SdkAppId { get; set; }

    /// <summary>
    /// AccessKey ID
    /// </summary>
    public string AccessKeyId { get; set; }

    /// <summary>
    /// AccessKey Secret
    /// </summary>
    public string AccessKeySecret { get; set; }

    /// <summary>
    /// Templates
    /// </summary>
    public List<SmsTemplate> Templates { get; set; }

    /// <summary>
    /// GetTemplate
    /// </summary>
    public SmsTemplate GetTemplate(string id = "0")
    {
        foreach (var template in Templates)
        {
            if (template.Id == id) { return template; }
        }
        return null;
    }
}

public class SmsTemplate
{
    public string Id { get; set; } = string.Empty;
    public string SignName { get; set; }
    public string TemplateCode { get; set; }
    public string Content { get; set; }
}