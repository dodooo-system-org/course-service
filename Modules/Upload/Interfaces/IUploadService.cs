using System;
using Imagekit.Models;

namespace course_service.Modules.Upload.Interfaces;

public interface IUploadService
{
    Task<AuthParamResponse> GenerateSignatureAsync();
}
