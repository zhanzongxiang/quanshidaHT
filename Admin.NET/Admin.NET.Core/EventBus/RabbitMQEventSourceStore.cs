﻿// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Channels;

namespace Admin.NET.Core;

/// <summary>
/// RabbitMQ自定义事件源存储器
/// </summary>
public class RabbitMQEventSourceStore : IEventSourceStorer, IDisposable
{
    /// <summary>
    /// 内存通道事件源存储器
    /// </summary>
    private Channel<IEventSource> _channelEventSource;

    /// <summary>
    /// 路由键
    /// </summary>
    private string _routeKey;

    /// <summary>
    /// 连接对象
    /// </summary>
    private IConnection _connection;

    /// <summary>
    /// 通道对象
    /// </summary>
    private IChannel _channel;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="factory">连接工厂</param>
    /// <param name="routeKey">路由键</param>
    /// <param name="capacity">存储器最多能够处理多少消息，超过该容量进入等待写入</param>
    public RabbitMQEventSourceStore(ConnectionFactory factory, string routeKey, int capacity)
    {
        InitEventSourceStore(factory, routeKey, capacity).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 初始化事件源存储器
    /// </summary>
    /// <param name="factory">连接工厂</param>
    /// <param name="routeKey">路由键</param>
    /// <param name="capacity">存储器最多能够处理多少消息，超过该容量进入等待写入</param>
    private async Task InitEventSourceStore(ConnectionFactory factory, string routeKey, int capacity)
    {
        // 配置通道（超出默认容量后进入等待）
        var boundedChannelOptions = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        // 创建有限容量通道
        _channelEventSource = Channel.CreateBounded<IEventSource>(boundedChannelOptions);

        // 创建连接
        _connection = await factory.CreateConnectionAsync();
        // 路由键名
        _routeKey = routeKey;

        // 创建通道
        _channel = await _connection.CreateChannelAsync();

        // 声明路由队列
        await _channel.QueueDeclareAsync(routeKey, false, false, false, null);

        // 创建消息订阅者
        var consumer = new AsyncEventingBasicConsumer(_channel);

        // 订阅消息并写入内存 Channel
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            // 读取原始消息
            var stringEventSource = Encoding.UTF8.GetString(ea.Body.ToArray());

            // 转换为 IEventSource，如果自定义了 EventSource，注意属性是可读可写
            var eventSource = JSON.Deserialize<ChannelEventSource>(stringEventSource);

            // 写入内存管道存储器
            await _channelEventSource.Writer.WriteAsync(eventSource);

            // 确认该消息已被消费
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        // 启动消费者且设置为手动应答消息
        await _channel.BasicConsumeAsync(routeKey, false, consumer);
    }

    /// <summary>
    /// 将事件源写入存储器
    /// </summary>
    /// <param name="eventSource">事件源对象</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns><see cref="ValueTask"/></returns>
    public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    {
        if (eventSource == default)
            throw new ArgumentNullException(nameof(eventSource));

        // 判断是否是 ChannelEventSource 或自定义的 EventSource
        if (eventSource is ChannelEventSource source)
        {
            // 序列化及发布
            var data = Encoding.UTF8.GetBytes(JSON.Serialize(source));
            var props = new BasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = DeliveryModes.Persistent;
            await _channel.BasicPublishAsync("", _routeKey, false, props, data);
        }
        else
        {
            // 处理动态订阅
            await _channelEventSource.Writer.WriteAsync(eventSource, cancellationToken);
        }
    }

    /// <summary>
    /// 从存储器中读取一条事件源
    /// </summary>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns>事件源对象</returns>
    public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    {
        var eventSource = await _channelEventSource.Reader.ReadAsync(cancellationToken);
        return eventSource;
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}