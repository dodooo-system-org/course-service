// Attributes/AuthorizationAttributes.cs
using course_service.Shared.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;

namespace course_service.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authInfo = context.HttpContext.Items["Auth"] as AuthInfoDto;
        if (authInfo?.Role == "admin")
        {
            return; // User is an admin, allow access
        }
        throw new UnauthorizedAccessException("You do not have permission");
    }
}