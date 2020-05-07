using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Basic.AuthReq
{
    public class CustomRequireClaim : IAuthorizationRequirement
    {
        public CustomRequireClaim(string claimType) { ClaimType = claimType; }

        public string ClaimType { get; }
    }

    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequireClaim>
    {
        public CustomRequireClaimHandler()
        {
            // db
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       CustomRequireClaim requirement)
        {
            var hasaClaim = context.User.Claims.Any(a => a.Type == requirement.ClaimType);

            if (hasaClaim)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public static class AuthPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireCustomClaim(this AuthorizationPolicyBuilder builder,
                                                                    string claimType)
        {
            builder.AddRequirements((new CustomRequireClaim(claimType)));

            return builder;
        }
    }
}
