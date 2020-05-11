using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index() { return View(); }

        [Authorize]
        public IActionResult Secret() { return View(); }

        public IActionResult Auth()
        {
            var claims = new[] {new Claim(JwtRegisteredClaimNames.Sub, "sub_id"), new Claim("custom", "cookie"),};
           
            var signCred = new SigningCredentials(ConfigConstants.GetKey(), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(ConfigConstants.Issuer,
                                             ConfigConstants.Audiance,
                                             claims,
                                             notBefore: DateTime.Now,
                                             expires: DateTime.Now.AddDays(1),
                                             signCred);

            var json = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new  {token = json});
        }
        public IActionResult Decode(string part)
        {
            var bytes = Convert.FromBase64String(part);
            return Ok(Encoding.UTF8.GetString(bytes));
        }
    }
}
