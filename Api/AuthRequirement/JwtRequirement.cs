using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Api.AuthRequirement
{
    public class JwtRequirement : IAuthorizationRequirement {}

    public class JwtRequirementHandler : AuthorizationHandler<JwtRequirement>
    {
        private readonly HttpClient _client;
        private readonly HttpContext _httpContext;

        public JwtRequirementHandler(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                             JwtRequirement requirement)
        {
            if (_httpContext.Request.Headers.TryGetValue("Authorization", out var header))
            {
                _client.DefaultRequestHeaders.Add("Authorization", header.ToString());
                var response = await _client.GetAsync("https://localhost:44345/oauth/validate");

                if (response.StatusCode == HttpStatusCode.OK)
                    context.Succeed(requirement);
            }
        }
    }
}
