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

            string username = resultContext.HttpContext.User.GetUsername();
            IUserRepository repository = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            AppUser user = await repository.GetUserByUsernameAsync(username);
            user.LastActive = DateTime.Now;
            await repository.SaveAllAsync();
        }
    }
}