// Attributes/AuthorizationAttributes.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace course_service.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {

    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAuthenticated = context.HttpContext.Items["IsAuthenticated"] as bool?;
        var userRole = context.HttpContext.Items["UserRole"] as string;

        if (isAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        if (userRole != "admin")
        {
            context.Result = new ForbidResult();
        }
    }
}