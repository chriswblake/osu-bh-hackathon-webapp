using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HackathonWebApp.Models;

namespace HackathonWebApp.Controllers
{
    public class AccountController : Controller
    {
        // Fields
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;

        // Constructor
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // Views
        [Authorize]
        public ViewResult Index()
        {
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;
            return View(appUser);
        }
        public IActionResult Login()
        {
            return View();
        }
        public ViewResult CreateUser() => View();
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Methods
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.Name,
                    Email = user.Email
                };

                // Create User
                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);

                // Inform User
                if (result.Succeeded)
                    ViewBag.Message = "User Created Successfully";
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string empname)
        {
            ApplicationUser appUser = await userManager.FindByNameAsync(User.Identity.Name);

            // Delete the user
            if (appUser != null)
            {
                IdentityResult result = await userManager.DeleteAsync(appUser);
                if (result.Succeeded)
                {
                    // Sign out and display confirmation
                    await signInManager.SignOutAsync();
                    return View();
                }
                else
                    Errors(result);
                }
            else
                ModelState.AddModelError("", "No role found");

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required][EmailAddress] string email, [Required] string password, string returnurl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = await userManager.FindByEmailAsync(email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(appUser, password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnurl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(email), "Login Failed: Invalid Email or Password");
            }

            return View();
        }
        
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}
