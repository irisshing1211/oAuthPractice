using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Server
{
    /// <summary>
    /// should all set in appsetting.json
    /// </summary>
    public class ConfigConstants
    {
        public const string Issuer = Audiance;
        public const string Audiance = "https://localhost:44345/";
        public const string Secret = "not_too_short_secret_otherwise_it_might_error";

        public static SymmetricSecurityKey GetKey()
        {
            var secretByte = Encoding.UTF8.GetBytes(Secret);

            return new SymmetricSecurityKey(secretByte);
        }
    }
}
