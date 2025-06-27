using course_service.Shared.RMQ;
using course_service.Shared.Services.RabbitMQ.DTOs;

namespace course_service.Examples;

public class TokenValidationSubscriber
{
    private readonly RMQService _rmqService;

    public TokenValidationSubscriber(RMQService rmqService)
    {
        _rmqService = rmqService;
    }

    public async Task StartListening()
    {
        // Subscribe to token validation responses
        await _rmqService.Subscribe<TokenValidationResponse>(
            routingKey: "auth.token.validation.response",
            messageHandler: HandleTokenValidationResponse
        );
    }

    private async Task HandleTokenValidationResponse(TokenValidationResponse response)
    {
        Console.WriteLine($"Received token validation response:");
        Console.WriteLine($"Correlation ID: {response.CorrelationId}");
        Console.WriteLine($"Is Valid: {response.IsValid}");

        if (response.Auth != null)
        {
            Console.WriteLine($"Auth ID: {response.Auth.AuthId}");
            Console.WriteLine($"Email: {response.Auth.Email}");
            Console.WriteLine($"Username: {response.Auth.Username}");
            Console.WriteLine($"Status: {response.Auth.Status}");
            Console.WriteLine($"Role: {response.Auth.Role}");
        }

        if (!string.IsNullOrEmpty(response.Error))
        {
            Console.WriteLine($"Error: {response.Error}");
        }

        // Process the response as needed
        // For example, update user authentication state, log the result, etc.
    }
}
