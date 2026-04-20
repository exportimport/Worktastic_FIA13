using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Worktastic.filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthentication : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.HttpContext.Request.Headers.TryGetValue("ApiKey", out var key))
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiKey = config.GetValue<string>("ApiKey");
                if (!key.Equals(apiKey))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            } else
            {
                context.Result = new UnauthorizedResult();
                return;
            }
                await next();
        }
    }
}
