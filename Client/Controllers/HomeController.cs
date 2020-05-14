using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        // GET
        public IActionResult Index() { return View(); }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            // check if token is ok
            var serverResponse = await AccessTokenWapper(() => GetRequest("https://localhost:44345/secret/index"));
            // then do what should do
            var apiResponse = await AccessTokenWapper(() => GetRequest("https://localhost:44390/secret/index"));
            var body = await apiResponse.Content.ReadAsStringAsync();

            return View(model: body);
        }

        /// <summary>
        /// get request to url with access_token header
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetRequest(string url)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                return await client.GetAsync(url);
            }
        }

        /// <summary>
        /// check if the request id unauthorized, if yes, then refresh token
        /// else return response
        /// </summary>
        /// <param name="initRequest"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> AccessTokenWapper(Func<Task<HttpResponseMessage>> initRequest)
        {
            var response = await initRequest();

            // if request is 401, then refresh token
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await RefreshAccessToken();
                response = await initRequest();
            }

            return response;
        }

        /// <summary>
        /// this should only use in server to server, for secure reason
        /// </summary>
        /// <returns></returns>
        private async Task RefreshAccessToken()
        {
            // first get the refresh token, with should be "RefreshTokenSampleValueSomething77" in the OAuth controller
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            using (var refreshTokenClient = _httpClientFactory.CreateClient())
            {

                var data = new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44345/oauth/token")
                {
                    Content = new FormUrlEncodedContent(data)
                };

                /*var basicCred = "username:password";
                var encoded = Encoding.UTF8.GetBytes(basicCred);
                var base64 = Convert.ToBase64String(encoded);
                request.Headers.Add("Authorization", $"Bearer {base64}");*/
                var response = await refreshTokenClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                var newAccessToken = responseData.GetValueOrDefault("access_token");
                var newRefreshToken = responseData.GetValueOrDefault("refresh_token");
                var authInfo = await HttpContext.AuthenticateAsync("ClientCookie");

                // update token to new refresh token
                authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
                authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

                // sign in again using new refresh token
                await HttpContext.SignInAsync("ClientCookie", authInfo.Principal, authInfo.Properties);
            }
        }
    }
}
