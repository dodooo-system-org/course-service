using System;
using RabbitMQ.Client;

namespace course_service.Shared.RMQ.Interfaces;

public interface IRMQService
{
    Task PublishMessage<T>(string exchangeName, string exchangeType, string routingKey, BasicProperties basicProperties, T message) where T : class;

    Task Subscribe<T>(string routingKey, Func<T, Task>? messageHandler) where T : class;
    void Dispose();
}
