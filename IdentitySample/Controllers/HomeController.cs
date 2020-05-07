using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentitySample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET
        public IActionResult Index() { return View(); }

        [Authorize]
        public IActionResult Secret() { return View(); }

        public IActionResult Login() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // check user exist
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                // sign in
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded) { return RedirectToAction("Index"); }
            }

            // can access
            return RedirectToAction("Index");
        }

        public IActionResult Reg() => View();

        [HttpPost]
        public async Task<IActionResult> Reg(string username, string password)
        {
            var user = new IdentityUser {UserName = username, Email = ""};
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // sign user here
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded) { return RedirectToAction("Index"); }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index");
        }
    }
}
