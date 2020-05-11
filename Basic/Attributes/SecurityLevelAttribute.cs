using System.Net;
using Basic.CustomAuthorizationPolicy;
using Microsoft.AspNetCore.Authorization;

namespace Basic.Attributes
{
    public class SecurityLevelAttribute : AuthorizeAttribute
    {
        public SecurityLevelAttribute(int level) { Policy = $"{DynamicPolices.SecurityLevel}.{level}"; }
    }
}
