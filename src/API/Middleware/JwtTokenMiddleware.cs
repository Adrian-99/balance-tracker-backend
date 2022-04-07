using Application.Interfaces;

namespace API.Middleware
{
    public class JwtTokenMiddleware
    {
        private RequestDelegate next;
        private IJwtService jwtService;

        public JwtTokenMiddleware(RequestDelegate next, IJwtService jwtService)
        {
            this.next = next;
            this.jwtService = jwtService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.Authorization.Count > 0)
            {
                string accessToken = context.Request.Headers.Authorization.First().Split(' ').Last();
                var authorizedUsername = await jwtService.ValidateAccessToken(accessToken);
                context.Items["authorizedUsername"] = authorizedUsername;
            }
            await next.Invoke(context);
        }
    }
}
