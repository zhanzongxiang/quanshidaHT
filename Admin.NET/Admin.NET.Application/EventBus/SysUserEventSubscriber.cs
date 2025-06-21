﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Furion.EventBus;

namespace Admin.NET.Application;

/// <summary>
/// 系统用户操作事件订阅
/// </summary>
public class SysUserEventSubscriber : IEventSubscriber, ISingleton, IDisposable
{
    public SysUserEventSubscriber()
    {
    }

    /// <summary>
    /// 增加系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Add)]
    public Task AddUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Register)]
    public Task RegisterUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Update)]
    public Task UpdateUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Delete)]
    public Task DeleteUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 设置系统用户状态
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.SetStatus)]
    public Task SetUserStatus(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.UpdateRole)]
    public Task UpdateUserRole(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 解除登录锁定
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.UnlockLogin)]
    public Task UnlockUserLogin(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
    }
}