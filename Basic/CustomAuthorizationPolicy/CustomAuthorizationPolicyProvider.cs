using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Basic.CustomAuthorizationPolicy
{
    public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyName">{claim name}.{value}</param>
        /// <returns></returns>
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            foreach (var customPolicy in DynamicPolices.Get())
            {
                if (policyName.StartsWith(customPolicy))
                {
                    var policy = DynamicAuthorizationPolicyFactory.Create(policyName);

                    return Task.FromResult(policy);
                }
            }

            return base.GetPolicyAsync(policyName);
        }
    }

    /// <summary>
    /// claim name, policy name
    /// {claim name}
    /// </summary>
    public static class DynamicPolices
    {
        public const string SecurityLevel = "SecurtiyLevel";
        public const string Rank = "Rank";

        /// <summary>
        /// 
        /// </summary>
        /// <returns>[SecurtiyLevel,Rank]</returns>
        public static IEnumerable<string> Get()
        {
            yield return SecurityLevel;
            yield return Rank;
        }
    }

    /// <summary>
    /// to create dynamic AuthorizationPolicyBuilder by policy name
    /// </summary>
    public static class DynamicAuthorizationPolicyFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyName">{claim name}.{value}</param>
        /// <returns></returns>
        public static AuthorizationPolicy Create(string policyName)
        {
            var arr = policyName.Split('.');
            var type = arr.First();
            var value = arr.Last();

            switch (type)
            {
                case DynamicPolices.Rank:
                    return new AuthorizationPolicyBuilder().RequireClaim("Rank", value).Build();
                case DynamicPolices.SecurityLevel:
                    return new AuthorizationPolicyBuilder()
                           .AddRequirements(new SecurityLevelRequirement(Convert.ToInt32(value)))
                           .Build();
                default:
                    return null;
            }
        }
    }

    #region SecurityLevel

    public class SecurityLevelRequirement : IAuthorizationRequirement
    {
        public int Level { get; }

        public SecurityLevelRequirement(int level) { Level = level; }
    }

    public class SecurityLevelHandler : AuthorizationHandler<SecurityLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SecurityLevelRequirement requirement)
        {
            // get SecurityLevel(Claim) value
            var value = Convert.ToInt32(context.User.Claims.FirstOrDefault(a => a.Type == DynamicPolices.SecurityLevel)
                                               ?.Value ??
                                        "0");

            // check if claim value >= required level, then ok
            if (requirement.Level <= value)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    #endregion
}
