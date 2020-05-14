using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Server.Controllers
{
    public class OAuthController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response_type">auth flow type</param>
        /// <param name="client_id"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="scope">what info need in the access token</param>
        /// <param name="state">random string generated which use to confirm user are going back to the same client</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Auth(string response_type,
                                  string client_id,
                                  string redirect_uri,
                                  string scope,
                                  string state)
        {
            // ?a=foo&b=bar
            var query = new QueryBuilder {{"redirectUri", redirect_uri}, {"state", state}};

            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Auth(string username, string redirectUri, string state)
        {
            // here should be a login function
            const string code = "some_code";
            var query = new QueryBuilder {{"code", code}, {"state", state}};

            // if login success then generate a token from the config.TokenEndpoint then redirect to the uri
            return Redirect($"{redirectUri}{query.ToString()}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grant_type">flow of access_token request</param>
        /// <param name="code">confirmation of the authentication process</param>
        /// <param name="redirect_uri"></param>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Token(string grant_type,
                                               string code,
                                               string redirect_uri,
                                               string client_id,
                                               string refresh_token)
        {
            // validate the code here
            // ...
            var claims = new[] {new Claim(JwtRegisteredClaimNames.Sub, "sub_id"), new Claim("custom", "cookie"),};
            var signCred = new SigningCredentials(ConfigConstants.GetKey(), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(ConfigConstants.Issuer,
                                             ConfigConstants.Audiance,
                                             claims,
                                             notBefore: DateTime.Now,
                                             expires: grant_type == "refresh_token"
                                                 ? DateTime.Now.AddMinutes(5)
                                                 : DateTime.Now.AddMinutes(1),
                                             signCred);

            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            #region write token to response

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthTutorial",
                refresh_token = "RefreshTokenSampleValueSomething77"
            };

            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);

            #endregion

            return Redirect(redirect_uri);
        }

        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var header)) { return Ok(); }

            return BadRequest();
        }
    }
}
