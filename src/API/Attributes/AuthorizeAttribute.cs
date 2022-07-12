using Application.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly bool requireVerifiedEmail;

        public AuthorizeAttribute(bool requireVerifiedEmail = true)
        {
            this.requireVerifiedEmail = requireVerifiedEmail;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return;
            }

            if (context.HttpContext.Items[Constants.AUTHORIZED_USERNAME] == null)
            {
                context.Result = new JsonResult(ApiResponse<string>.Error(
                    "Unauthorized"
                    // TODO: Add translation key
                ))
                { ContentType = "application/json", StatusCode = StatusCodes.Status401Unauthorized };
            }

            if (requireVerifiedEmail && (bool)context.HttpContext.Items[Constants.IS_EMAIL_VERIFIED] == false)
            {
                context.Result = new JsonResult(ApiResponse<string>.Error(
                    "Forbidden - verified email required"
                    // TODO: Add translation key
                ))
                { ContentType = "application/json", StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
