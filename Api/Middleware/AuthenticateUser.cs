using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Middleware;

public class AuthenticateUser : Attribute, IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var user = context.HttpContext.User;
        var email = user.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized: Email claim missing");
        }
        context.HttpContext.Items["Email"] = email;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        
    }
}