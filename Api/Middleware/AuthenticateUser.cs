using System.Security.Claims;

namespace Api.Middleware;

public class AuthenticateUser
{
    private readonly RequestDelegate _next;

    public AuthenticateUser(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path is not null &&
            (path.StartsWith("/swagger") ||      
             path.StartsWith("/favicon")||
             path.StartsWith("/auth/register")||
             path.StartsWith("/auth/login"))) 
        {
            await _next(context);
            return;
        }
        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorised: JWT token is missing or invalid.");
            }

            return;
        }
        var email = context.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorised: Email claim not found");
            }

            return;
        }
        context.Items["Email"] = email;
        await _next(context);
    }
     
}