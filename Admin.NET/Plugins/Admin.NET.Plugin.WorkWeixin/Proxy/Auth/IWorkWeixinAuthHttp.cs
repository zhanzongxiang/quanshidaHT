﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Plugin.WorkWeixin.Proxy.AppChat;

/// <summary>
/// 授权会话远程服务
/// </summary>
public interface IWorkWeixinAuthHttp : IHttpDeclarative
{
    /// <summary>
    /// 获取接口凭证
    /// </summary>
    /// <param name="corpId">企业ID</param>
    /// <param name="corpSecret">应用的凭证密钥</param>
    /// <returns></returns>
    /// <inheritdoc cref="https://developer.work.weixin.qq.com/document/path/91039"/>
    [Post("https://qyapi.weixin.qq.com/cgi-bin/gettoken")]
    Task<AuthAccessTokenHttpOutput> GetToken([Query("corpid")] string corpId, [Query("corpsecret")] string corpSecret);
}