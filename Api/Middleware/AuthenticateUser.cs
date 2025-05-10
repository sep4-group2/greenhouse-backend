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
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorised: JWT token is missing or invalid.");
        }
        var email = context.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorised: Email claim not found");
        }
        context.Items["Email"] = email;
        await _next(context);
    }
     
}