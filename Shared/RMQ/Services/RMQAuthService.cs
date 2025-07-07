using System;
using System.Collections.Concurrent;
using course_service.Shared.DTOs;
using course_service.Shared.RMQ.Interfaces;
using course_service.Shared.Services.RabbitMQ.DTOs;
using RabbitMQ.Client;

namespace course_service.Shared.RMQ.Services;

public class RMQAuthService : IRMQAuthService
{
    private readonly IRMQService _rmqService;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<TokenValidationResponse>> _pendingRequests = new();
    public RMQAuthService(IRMQService rmqService)
    {
        _rmqService = rmqService;

        // Subscribe to the response queue
        _rmqService.Subscribe<TokenValidationResponse>("course.auth.token-validation-response", async response =>
        {
            if (_pendingRequests.TryRemove(response.CorrelationId, out var tsc))
            {
                tsc.SetResult(response);
            }
        });
    }

    private TokenValidationRequest CreateTokenValidationRequest(string token)
    {
        string correlationId = Guid.NewGuid().ToString();
        return new TokenValidationRequest
        {
            CorrelationId = correlationId,
            Token = token
        };
    }
    public async Task<TokenValidationResponse> ValidateTokenAsync(string token)
    {
        var data = this.CreateTokenValidationRequest(token);
        var message = new NestJSMessagePayload<TokenValidationRequest>
        {
            Pattern = "user.auth.token-validation-request",
            Data = data
        };
        var basicProperties = new BasicProperties
        {
            ReplyTo = "course.auth.token-validation-response",
            CorrelationId = data.CorrelationId,
        };
        await _rmqService.PublishMessage("amq.topic", "topic", "user.auth.token-validation-request", basicProperties, message);

        var tsc = new TaskCompletionSource<TokenValidationResponse>();

        _pendingRequests.TryAdd(data.CorrelationId, tsc);

        // Set a timeout for the task completion
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        try
        {
            var result = await tsc.Task.WaitAsync(cts.Token);
            return result;
        }
        catch
        {
            _pendingRequests.TryRemove(data.CorrelationId, out _);
            // To throw unhandled exception if the task does not complete within the timeout
            throw new Exception();
        }
    }
}
