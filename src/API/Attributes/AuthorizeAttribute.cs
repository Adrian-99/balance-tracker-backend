using Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return;
            }

            if (context.HttpContext.Items["authorizedUser"] == null)
            {
                context.Result = new JsonResult(new ActionResultDto(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized"
                    // TODO: Add translation key
                ))
                { ContentType = "application/json", StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
