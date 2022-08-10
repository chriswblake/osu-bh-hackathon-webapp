using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HackathonWebApp.Models;
using System;
using System.Net.Mail;
using System.IO;

namespace HackathonWebApp.Controllers
{
    public class AccountController : Controller
    {
        // Fields
        private IWebHostEnvironment webHostEnvironment;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private SmtpClient emailClient;

        // Constructor
        public AccountController(IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, SmtpClient emailClient)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailClient = emailClient;
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
        public ViewResult Create() => View();
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Methods
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser appUser)
        {
            if (ModelState.IsValid)
            {
                // Check if user exists
                ApplicationUser existingUser = await userManager.FindByEmailAsync(appUser.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email already in use.");
                    return View(appUser);
                }

                // Create User
                IdentityResult result = await userManager.CreateAsync(appUser, appUser.Password);

                // Send confirmation email to user
                if (result.Succeeded)
                {
                    // Generate confirmation email body with token
                    string templatePath = Path.Combine(webHostEnvironment.WebRootPath, "email-templates", "ConfirmEmailAddress.html");
                    string msgBodyTemplate = System.IO.File.ReadAllText(templatePath);
                    string code = await userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = appUser.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    string userName = appUser.UserName;
                    string msgBody = string.Format(msgBodyTemplate, userName, callbackUrl);

                    // Send Email
                    MailMessage mail = new MailMessage();
                    mail.To.Add(appUser.Email);
                    mail.From = new MailAddress("hackokstate@gmail.com");
                    mail.Subject = "Confirm Account - HackOKState";
                    mail.Body = msgBody;
                    mail.IsBodyHtml = true;
                    await emailClient.SendMailAsync(mail);

                    // Go back to login page
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(appUser);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete()
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
                    return RedirectToAction("DeleteConfirmation", "Account");
                }
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");

            return RedirectToAction("Index");
        }
        public ViewResult DeleteConfirmation(){
            return View("Delete");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required][EmailAddress] string email, [Required] string password, string returnurl)
        {
            // If model is not complete, show login
            if (!ModelState.IsValid)
                return View();

            // Check if user exists
            ApplicationUser appUser = await userManager.FindByEmailAsync(email);
            if (appUser == null)
            {
                ModelState.AddModelError(nameof(email), "Login Failed: Invalid Email or Password");
                return View();
            }

            // Check if email is confirmed
            bool emailConfirmed = await userManager.IsEmailConfirmedAsync(appUser);
            if (!emailConfirmed)
            {
                ModelState.AddModelError(nameof(email), "Login Failed: Unconfirmed Email");
                return View();
            }

            // Try to login
            Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(appUser, password, false, false);
            if (result.Succeeded)
            { 
                return Redirect(returnurl ?? "/");
            }
            else
            {
               ModelState.AddModelError(nameof(email), "Login Failed: Invalid Email or Password");
                return View();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUser appUserChanges)
        {
            ApplicationUser appUser = null;
            try
            {
                // Find the user
                appUser = await userManager.FindByNameAsync(User.Identity.Name);

                // Update basic info
                appUser.FirstName = appUserChanges.FirstName;
                appUser.LastName = appUserChanges.LastName;
                appUser.UserName = appUserChanges.UserName;
                appUser.Email = appUserChanges.Email;
                
                // Save to DB
                await userManager.UpdateAsync(appUser);

                // Sign in again, incase username (email) is different
                await signInManager.SignInAsync(appUser, true);
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return View("Index", appUser);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            // Get the user
            var appUser = await userManager.FindByIdAsync(userId);

            // Check if already confirmed
            bool emailAlreadyConfirmed = await userManager.IsEmailConfirmedAsync(appUser);
            if (emailAlreadyConfirmed)
            {
                ViewBag.EmailConfirmed = "already";
                return View();
            }

            // Try to confirm the email
            var result = await userManager.ConfirmEmailAsync(appUser, code);
            if (result.Succeeded)
                ViewBag.EmailConfirmed = "yes";
            else
                ViewBag.EmailConfirmed = "no";
            return View();
        }

        // Passsword Reset
        public ViewResult ForgotPassword() {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                // If user doesn't exist, fail silently to protect user.
                var appUser = await this.userManager.FindByEmailAsync(email);
                if (appUser == null || !(await this.userManager.IsEmailConfirmedAsync(appUser)))
                {
                    return View("ForgotPasswordConfirmation");
                }

                // Generate confirmation email body with token
                string templatePath = Path.Combine(webHostEnvironment.WebRootPath, "email-templates", "ForgotPassword.html");
                string msgBodyTemplate = System.IO.File.ReadAllText(templatePath);
                var code = await this.userManager.GeneratePasswordResetTokenAsync(appUser);
                var callbackUrl = Url.Action("ResetPassword", "Account",  new { UserId = appUser.Id, code = code }, protocol: HttpContext.Request.Scheme);
                string userName = appUser.UserName;
                string msgBody = string.Format(msgBodyTemplate, userName, callbackUrl);

                // Send Email
                MailMessage mail = new MailMessage();
                mail.To.Add(appUser.Email);
                mail.From = new MailAddress("hackokstate@gmail.com");
                mail.Subject = "Forgot Password - HackOKState";
                mail.Body = msgBody;
                mail.IsBodyHtml = true;
                await emailClient.SendMailAsync(mail);      
                
                // Show confirmation page
                return View(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(email);
        }
        public ViewResult ForgotPasswordConfirmation() {
            return View();
        }

        // Error Messages
        private void Errors(Task result)
        {
            var e = result.Exception;
            Errors(e);
        }
        private void Errors(Exception e)
        {
            ModelState.AddModelError("", e.Message);
        }
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}
