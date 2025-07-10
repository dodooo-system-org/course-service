using System;

namespace course_service.Shared.Helpers
{
    public static class ServiceErrorHelper
    {
        private static readonly Type[] _validErrorTypes = [
            typeof(UnauthorizedAccessException),
            typeof(ArgumentException),
            typeof(KeyNotFoundException),
            typeof(InvalidOperationException)
        ];
        public static Exception GenerateErrorService(Exception error, string? message = null)
        {
            if (IsControlledErrorService(error))
            {
                return error;
            }
            else if (message != null)
            {
                throw new InvalidOperationException(message, error);
            }
            else
            {
                return new Exception(message ?? "An unexpected error occurred", error);
            }
        }

        public static bool IsControlledErrorService(Exception error)
        {
            if (error == null)
            {
                return false;
            }
            if (!_validErrorTypes.Contains(error.GetType()))
            {
                return false;
            }
            return true;
        }
    }
}