using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ActionExecutedContext resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            int userId = resultContext.HttpContext.User.GetUserId();
            IUserRepository repository = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            AppUser user = await repository.GetUserByIdAsync(userId);
            user.LastActive = DateTime.Now;
            await repository.SaveAllAsync();
        }
    }
}