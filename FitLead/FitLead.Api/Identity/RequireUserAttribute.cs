using FitLead.Application.Common.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitLead.Api.Identity
{
    public sealed class RequireUserAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.RequestServices.GetRequiredService<IUserContext>();
            if (!user.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
