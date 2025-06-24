using System;

namespace course_service.Shared.Helpers
{
    public static class ServiceErrorHelper
    {
        public static Exception GenerateErrorService(Exception error, string? message = null)
        {
            if (error.GetType() == typeof(Exception))
            {
                return new Exception(message ?? "An unexpected error occurred", error);
            }
            else
            {
                return error;
            }
        }
    }
}