﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 开放接口身份输入参数
/// </summary>
public class OpenAccessInput : BasePageInput
{
    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; }
}

public class AddOpenAccessInput : SysOpenAccess
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [Required(ErrorMessage = "身份标识不能为空")]
    public override string AccessKey { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public override string AccessSecret { get; set; }

    /// <summary>
    /// 绑定用户Id
    /// </summary>
    [Required(ErrorMessage = "绑定用户不能为空")]
    public override long BindUserId { get; set; }
}

public class UpdateOpenAccessInput : AddOpenAccessInput
{
}

public class DeleteOpenAccessInput : BaseIdInput
{
}

public class GenerateSignatureInput
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [Required(ErrorMessage = "身份标识不能为空")]
    public string AccessKey { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public string AccessSecret { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public HttpMethodEnum Method { get; set; }

    /// <summary>
    /// 请求接口地址
    /// </summary>
    [Required(ErrorMessage = "请求接口地址不能为空")]
    public string Url { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [Required(ErrorMessage = "时间戳不能为空")]
    public long Timestamp { get; set; }

    /// <summary>
    /// 随机数
    /// </summary>
    [Required(ErrorMessage = "随机数不能为空")]
    public string Nonce { get; set; }
}