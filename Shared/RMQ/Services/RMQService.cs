using System;
using System.Text;
using System.Text.Json;
using course_service.Shared.RMQ.Interfaces;
using course_service.Shared.DTOs;
using DotNetEnv;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using course_service.Shared.Helpers;

namespace course_service.Shared.RMQ;

public class RMQService : IDisposable, IRMQService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger _logger;

    public RMQService()
    {
        _logger = LoggerHelper.GetLogger<RMQService>();

        Env.Load();

        var factory = new ConnectionFactory
        {
            HostName = Env.GetString("RABBITMQ_HOST"),
            Port = Env.GetInt("RABBITMQ_PORT"),
            UserName = Env.GetString("RABBITMQ_USER"),
            Password = Env.GetString("RABBITMQ_PASSWORD"),
            VirtualHost = Env.GetString("RABBITMQ_VIRTUAL_HOST", "/")
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Declare exchange
        _channel.ExchangeDeclareAsync(
            exchange: "amq.topic",
            type: "topic",
            durable: true,
            autoDelete: false,
            arguments: null
        ).GetAwaiter().GetResult();


        _channel.QueueDeclareAsync(
            queue: "course_service_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        ).GetAwaiter().GetResult();
    }

    public async Task PublishMessage<T>(string exchangeName, string exchangeType, string routingKey, BasicProperties basicProperties, T message) where T : class
    {
        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: exchangeType, durable: true, autoDelete: false, arguments: null);
            await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, mandatory: false, basicProperties: basicProperties, body: body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing message: {ex.Message}");
            throw;
        }
    }

    public async Task Subscribe<T>(string routingKey, Func<T, Task>? messageHandler) where T : class
    {
        // Bind the queue to the exchange with routing key
        await _channel.QueueBindAsync(
            queue: "course_service_queue",
            exchange: "amq.topic",
            routingKey: routingKey,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var jsonString = Encoding.UTF8.GetString(body);

                // Check if the payload is a Buffer object from NestJS
                if (jsonString.StartsWith("{\"type\":\"Buffer\",\"data\":["))
                {
                    // Parse the Buffer object
                    var bufferObject = JsonSerializer.Deserialize<BufferPayload>(jsonString);
                    if (bufferObject?.Data != null)
                    {
                        var actualJsonBytes = bufferObject?.Data?.Select(x => (byte)x).ToArray() ?? [];
                        jsonString = Encoding.UTF8.GetString(actualJsonBytes);
                    }
                }

                // Deserialize the actual message using the generic type T
                var message = JsonSerializer.Deserialize<T>(jsonString);
                if (message != null && messageHandler != null)
                {
                    await messageHandler(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "course_service_queue",
            autoAck: true,
            consumer: consumer
        );
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error disposing RabbitMQ resources: {ex.Message}");
        }
    }
}
