using System;
using course_service.Modules.Upload.Interfaces;
using course_service.Shared.Helpers;
using DotNetEnv;
using Imagekit.Models;
using Imagekit.Sdk;

namespace course_service.Modules.Upload.Services;

public class UploadService : IUploadService
{
    private readonly ImagekitClient _imagekit;
    private readonly ILogger _logger;

    public UploadService()
    {
        Env.Load();
        _imagekit = new ImagekitClient(Env.GetString("IMAGEKIT_PUBLIC_KEY"),
                                        Env.GetString("IMAGEKIT_PRIVATE_KEY"),
                                        Env.GetString("IMAGEKIT_URL_ENDPOINT"));
        _logger = LoggerHelper.GetLogger<UploadService>();
    }

    public Task<AuthParamResponse> GenerateSignatureAsync()
    {
        try
        {
            string token = Guid.NewGuid().ToString();
            // Convert to Unix timestamp (seconds since epoch)
            long expirationUnix = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(5)).ToUnixTimeSeconds();
            string expiration = expirationUnix.ToString();
            var authenticationParameters = _imagekit.GetAuthenticationParameters(token, expiration);
            return Task.FromResult(authenticationParameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload signature");
            throw ServiceErrorHelper.GenerateErrorService(ex, "Failed to generate upload signature");
        }
    }
}
