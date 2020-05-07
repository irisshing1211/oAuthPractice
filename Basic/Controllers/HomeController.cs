using System.Collections.Generic;
using System.Security.Claims;
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
        public IActionResult Auth()
        {
            // once login -> get all the info needed then
            var myClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Bob"),
                new Claim(ClaimTypes.Email, "bob@mail.com"),
                new Claim(ClaimTypes.DateOfBirth, "01/01/1991"),
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
    }
}
