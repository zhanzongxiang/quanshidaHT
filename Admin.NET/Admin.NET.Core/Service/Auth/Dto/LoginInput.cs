// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 用户登录参数
/// </summary>
public class LoginInput
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空"), MinLength(2, ErrorMessage = "账号不能少于2个字符")]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
    public string Password { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; }
}

public class LoginPhoneInput
{
    /// <summary>
    /// 手机号码
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "手机号码不能为空")]
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "手机号码不正确")]
    public string Phone { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空"), MinLength(4, ErrorMessage = "验证码不能少于4个字符")]
    public string Code { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    [Required(ErrorMessage = "租户不能为空")]
    public long? TenantId { get; set; }
}

/// <summary>
/// 用户注册输入参数
/// </summary>
public class UserRegistrationInput
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空"), MinLength(2, ErrorMessage = "真实姓名不能少于2个字符")]
    public string RealName { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空"), MinLength(6, ErrorMessage = "账号不能少于6个字符")]
    public string Account { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "手机号码不能为空")]
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "手机号码不正确")]
    public string Phone { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空")]
    public string Code { get; set; }

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    [Required(ErrorMessage = "租户不能为空")]
    public long TenantId { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 注册方案
    /// </summary>
    public long WayId { get; set; }
}