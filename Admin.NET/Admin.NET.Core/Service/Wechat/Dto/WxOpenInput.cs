﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core.Service;

/// <summary>
/// 获取微信用户OpenId
/// </summary>
public class JsCode2SessionInput
{
    /// <summary>
    /// JsCode
    /// </summary>
    [Required(ErrorMessage = "JsCode不能为空"), MinLength(10, ErrorMessage = "JsCode错误")]
    public string JsCode { get; set; }
}

/// <summary>
/// 获取微信用户电话号码
/// </summary>
public class WxPhoneInput : WxOpenIdLoginInput
{
    /// <summary>
    /// Code
    /// </summary>
    [Required(ErrorMessage = "Code不能为空"), MinLength(10, ErrorMessage = "Code错误")]
    public string Code { get; set; }
}

/// <summary>
/// 微信小程序登录
/// </summary>
public class WxOpenIdLoginInput
{
    /// <summary>
    /// OpenId
    /// </summary>
    [Required(ErrorMessage = "微信标识不能为空"), MinLength(10, ErrorMessage = "微信标识错误")]
    public string OpenId { get; set; }
}

/// <summary>
/// 微信手机号登录
/// </summary>
public class WxPhoneLoginInput
{
    /// <summary>
    /// 电话号码
    /// </summary>
    [DataValidation(ValidationTypes.PhoneNumber, ErrorMessage = "电话号码错误")]
    public string PhoneNumber { get; set; }
}

/// <summary>
/// 发送订阅消息
/// </summary>
public class SendSubscribeMessageInput
{
    /// <summary>
    /// 订阅模板Id
    /// </summary>
    [Required(ErrorMessage = "订阅模板Id不能为空")]
    public string TemplateId { get; set; }

    /// <summary>
    /// 接收者的OpenId
    /// </summary>
    [Required(ErrorMessage = "接收者的OpenId不能为空")]
    public string ToUserOpenId { get; set; }

    /// <summary>
    /// 模板内容，格式形如 { "key1": { "value": any }, "key2": { "value": any } }
    /// </summary>
    [Required(ErrorMessage = "模板内容不能为空")]
    public Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> Data { get; set; }

    /// <summary>
    /// 跳转小程序类型
    /// </summary>
    public string MiniprogramState { get; set; }

    /// <summary>
    /// 语言类型
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// 点击模板卡片后的跳转页面（仅限本小程序内的页面），支持带参数（示例pages/app/index?foo=bar）
    /// </summary>
    public string MiniProgramPagePath { get; set; }
}

/// <summary>
/// 增加订阅消息模板
/// </summary>
public class AddSubscribeMessageTemplateInput
{
    /// <summary>
    /// 模板标题Id
    /// </summary>
    [Required(ErrorMessage = "模板标题Id不能为空")]
    public string TemplateTitleId { get; set; }

    /// <summary>
    /// 模板关键词列表,例如 [3,5,4]
    /// </summary>
    [Required(ErrorMessage = "模板关键词列表不能为空")]
    public List<int> KeyworkIdList { get; set; }

    /// <summary>
    /// 服务场景描述，15个字以内
    /// </summary>
    [Required(ErrorMessage = "服务场景描述不能为空")]
    public string SceneDescription { get; set; }
}

/// <summary>
/// 生成带参数小程序二维码(总共生成的码数量限制为 100,000)
/// </summary>
public class GenerateQRImageInput
{
    /// <summary>
    /// 扫码进入的小程序页面路径，最大长度 128 个字符，不能为空； eg: pages/index?id=0001
    /// </summary>
    public string PagePath { get; set; }

    /// <summary>
    /// 文件保存的名称
    /// </summary>
    public string ImageName { get; set; }

    /// <summary>
    /// 图片宽度 默认430
    /// </summary>
    public int Width { get; set; } = 430;
}

/// <summary>
/// 生成带参数小程序二维码(获取不受限制的小程序码)
/// </summary>
public class GenerateQRImageUnLimitInput : GenerateQRImageInput
{
    /// <summary>
    /// 二维码携带的参数 eg:a=1（最大32个可见字符，只支持数字，大小写英文以及部分特殊字符：<!-- !#$&'()*+,/:;=?@-._~ -->）
    /// </summary>
    public string Scene { get; set; }
}