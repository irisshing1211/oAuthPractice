using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Basic.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index() { return View(); }

        [Authorize]
        public IActionResult Secret() { return View(); }

        [Authorize(Policy = "Claim.DoB")]
        public IActionResult SecretPolicy() { return View("Secret"); }

        [SecurityLevel(5)]
        public IActionResult SecretLevel() { return View("Secret"); }

        [SecurityLevel(7)]
        public IActionResult SecretHigherLevel() { return View("Secret"); }

        [Authorize(Roles = "Admin")]
        public IActionResult SecretRole() { return View("Secret"); }

        [AllowAnonymous]
        public IActionResult Auth()
        {
            // once login -> get all the info needed then
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Bob"),
                new Claim(ClaimTypes.Email, "bob@mail.com"),
                new Claim(ClaimTypes.DateOfBirth, "01/01/1991"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("Welcome", "Hello")
            };

            var licenseClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "bob K Foo"), new Claim("IdCard", "AA219874")
            };

            //generate an identity
            var identity = new ClaimsIdentity(myClaims, "My identity");
            var licenseIdentity = new ClaimsIdentity(licenseClaims, "Gov");
            var userPrincipal = new ClaimsPrincipal(new[] {identity, licenseIdentity});

            // 
            HttpContext.SignInAsync(userPrincipal);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DoStuff([FromServices] IAuthorizationService authService)
        {
            var builder = new AuthorizationPolicyBuilder("Schema");
            var customPolicy = builder.RequireClaim("hello").Build();
            var authResult = await authService.AuthorizeAsync(HttpContext.User, customPolicy);

            if (authResult.Succeeded) { return View("Index"); }

            return View("Index");
        }
    }
}
