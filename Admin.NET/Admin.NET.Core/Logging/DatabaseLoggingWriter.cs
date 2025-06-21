// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.Core;

/// <summary>
/// 数据库日志写入器
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly ISqlSugarClient _db;
    private readonly SysConfigService _sysConfigService; // 参数配置服务
    private readonly ILogger<DatabaseLoggingWriter> _logger; // 日志组件

    public DatabaseLoggingWriter(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
        //_db = _serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        _sysConfigService = _serviceScope.ServiceProvider.GetRequiredService<SysConfigService>();
        _logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<DatabaseLoggingWriter>>();

        // 切换日志独立数据库
        _db = SqlSugarSetup.ITenant.IsAnyConnection(SqlSugarConst.LogConfigId)
            ? SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.LogConfigId)
            : SqlSugarSetup.ITenant.GetConnectionScope(SqlSugarConst.MainConfigId);
    }

    public async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        var jsonStr = logMsg.Context?.Get("loggingMonitor")?.ToString();
        if (string.IsNullOrWhiteSpace(jsonStr))
        {
            await _db.Insertable(new SysLogOp
            {
                DisplayTitle = "自定义操作日志",
                LogDateTime = logMsg.LogDateTime,
                EventId = logMsg.EventId.Id,
                ThreadId = logMsg.ThreadId,
                TraceId = logMsg.TraceId,
                Exception = logMsg.Exception == null ? null : JSON.Serialize(logMsg.Exception),
                Message = logMsg.Message,
                LogLevel = logMsg.LogLevel,
                Status = "200",
            }).ExecuteCommandAsync();
            return;
        }

        var loggingMonitor = JSON.Deserialize<dynamic>(jsonStr);
        // 记录数据校验日志
        if (loggingMonitor.validation != null && !await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysValidationLog)) return;

        // 获取当前操作者
        string account = "", realName = "", userId = "", tenantId = "";
        if (loggingMonitor.authorizationClaims != null)
        {
            var map = (loggingMonitor.authorizationClaims as IEnumerable<dynamic>)
                !.ToDictionary(u => u.type.ToString(), u => u.value.ToString());
            account = map.GetValueOrDefault(ClaimConst.Account);
            realName = map.GetValueOrDefault(ClaimConst.RealName);
            tenantId = map.GetValueOrDefault(ClaimConst.TenantId);
            userId = map.GetValueOrDefault(ClaimConst.UserId);
        }

        // 优先获取 X-Forwarded-For 头部信息携带的IP地址（如nginx代理配置转发）
        var remoteIPv4 = ((JArray)loggingMonitor.requestHeaders).OfType<JObject>()
            .FirstOrDefault(header => (string)header["key"] == "X-Forwarded-For")?["value"]?.ToString();

        if (string.IsNullOrEmpty(remoteIPv4))
            remoteIPv4 = loggingMonitor.remoteIPv4;

        (string ipLocation, double? longitude, double? latitude) = CommonUtil.GetIpAddress(remoteIPv4);

        var browser = "";
        var os = "";
        if (loggingMonitor.userAgent != null)
        {
            var client = Parser.GetDefault().Parse(loggingMonitor.userAgent.ToString());
            browser = $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor} / {client.Device.Family}";
            os = $"{client.OS.Family} {client.OS.Major} {client.OS.Minor}";
        }

        // 捕捉异常，否则会由于 unhandled exception 导致程序崩溃
        try
        {
            // 记录异常日志-发送邮件
            if (logMsg.Exception != null || loggingMonitor.exception != null)
            {
                await _db.Insertable(new SysLogEx
                {
                    ControllerName = loggingMonitor.controllerName,
                    ActionName = loggingMonitor.actionTypeName,
                    DisplayTitle = loggingMonitor.displayTitle,
                    Status = loggingMonitor.returnInformation?.httpStatusCode,
                    RemoteIp = remoteIPv4,
                    Location = ipLocation,
                    Longitude = longitude,
                    Latitude = latitude,
                    Browser = browser, // loggingMonitor.userAgent,
                    Os = os, // loggingMonitor.osDescription + " " + loggingMonitor.osArchitecture,
                    Elapsed = loggingMonitor.timeOperationElapsedMilliseconds,
                    LogDateTime = logMsg.LogDateTime,
                    Account = account,
                    RealName = realName,
                    HttpMethod = loggingMonitor.httpMethod,
                    RequestUrl = loggingMonitor.requestUrl,
                    RequestParam = (loggingMonitor.parameters == null || loggingMonitor.parameters.Count == 0) ? null : JSON.Serialize(loggingMonitor.parameters[0].value),
                    ReturnResult = loggingMonitor.returnInformation == null ? null : JSON.Serialize(loggingMonitor.returnInformation),
                    EventId = logMsg.EventId.Id,
                    ThreadId = logMsg.ThreadId,
                    TraceId = logMsg.TraceId,
                    Exception = JSON.Serialize(loggingMonitor.exception),
                    Message = logMsg.Message,
                    CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                    TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                    LogLevel = logMsg.LogLevel
                }).ExecuteCommandAsync();

                // 将异常日志发送到邮件
                if (await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysErrorMail))
                {
                    await App.GetRequiredService<IEventPublisher>().PublishAsync(CommonConst.SendErrorMail, logMsg.Exception ?? loggingMonitor.exception);
                }

                return;
            }

            // 记录访问日志-登录退出
            if (loggingMonitor.actionName == "userInfo" || loggingMonitor.actionName == "logout")
            {
                await _db.Insertable(new SysLogVis
                {
                    ControllerName = loggingMonitor.controllerName,
                    ActionName = loggingMonitor.actionTypeName,
                    DisplayTitle = loggingMonitor.displayTitle,
                    Status = loggingMonitor.returnInformation?.httpStatusCode,
                    RemoteIp = remoteIPv4,
                    Location = ipLocation,
                    Longitude = longitude,
                    Latitude = latitude,
                    Browser = browser, // loggingMonitor.userAgent,
                    Os = os, // loggingMonitor.osDescription + " " + loggingMonitor.osArchitecture,
                    Elapsed = loggingMonitor.timeOperationElapsedMilliseconds,
                    LogDateTime = logMsg.LogDateTime,
                    Account = account,
                    RealName = realName,
                    CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                    TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                    LogLevel = logMsg.LogLevel
                }).ExecuteCommandAsync();
                return;
            }

            // 记录操作日志
            if (!await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysOpLog)) return;
            await _db.Insertable(new SysLogOp
            {
                ControllerName = loggingMonitor.controllerName,
                ActionName = loggingMonitor.actionTypeName,
                DisplayTitle = loggingMonitor.displayTitle,
                Status = loggingMonitor.returnInformation?.httpStatusCode,
                RemoteIp = remoteIPv4,
                Location = ipLocation,
                Longitude = longitude,
                Latitude = latitude,
                Browser = browser, // loggingMonitor.userAgent,
                Os = os, // loggingMonitor.osDescription + " " + loggingMonitor.osArchitecture,
                Elapsed = loggingMonitor.timeOperationElapsedMilliseconds,
                LogDateTime = logMsg.LogDateTime,
                Account = account,
                RealName = realName,
                HttpMethod = loggingMonitor.httpMethod,
                RequestUrl = loggingMonitor.requestUrl,
                RequestParam = (loggingMonitor.parameters == null || loggingMonitor.parameters.Count == 0) ? null : JSON.Serialize(loggingMonitor.parameters[0].value),
                ReturnResult = loggingMonitor.returnInformation == null ? null : JSON.Serialize(loggingMonitor.returnInformation),
                EventId = logMsg.EventId.Id,
                ThreadId = logMsg.ThreadId,
                TraceId = logMsg.TraceId,
                Exception = loggingMonitor.exception == null ? null : JSON.Serialize(loggingMonitor.exception),
                Message = logMsg.Message,
                CreateUserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
                TenantId = string.IsNullOrWhiteSpace(tenantId) ? 0 : long.Parse(tenantId),
                LogLevel = logMsg.LogLevel
            }).ExecuteCommandAsync();

            await Task.Delay(50); // 延迟 0.05 秒写入数据库，有效减少高频写入数据库导致死锁问题
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "操作日志入库");
        }
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}