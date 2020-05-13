using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        public HomeController(IHttpClientFactory httpClientFactory) { _client = httpClientFactory.CreateClient(); }

        // GET
        public IActionResult Index() { return View(); }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var apiResponse = await _client.GetAsync("https://localhost:44390/secret/index");
            var body = await apiResponse.Content.ReadAsStringAsync();

            return View(model: body);
        }
    }
}
