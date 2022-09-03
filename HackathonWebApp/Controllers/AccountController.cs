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
using MongoDB.Driver;

namespace HackathonWebApp.Controllers
{
    public class AccountController : Controller
    {
        // Fields
        private IWebHostEnvironment webHostEnvironment;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private SmtpClient emailClient;
        private IMongoCollection<HackathonEvent> eventCollection;

        // Constructor
        public AccountController(IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, SmtpClient emailClient, IMongoDatabase database)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailClient = emailClient;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");
        }

        // Properties
        private HackathonEvent activeEvent {
            get {
                var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
                return eventController.activeEvent;
            }
        }

        // Account - Create and confirm account
        [Authorize]
        public ViewResult Index()
        {
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

            // Add event application, if they have one
            if (this.activeEvent.EventApplications.ContainsKey(appUser.Id.ToString()))
                ViewBag.EventApplication = this.activeEvent.EventApplications[appUser.Id.ToString()];
    
            return View(appUser);
        }
        public IActionResult Create()
        {
            return View();
        }
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
                    // Send email
                    await this.ConfirmAccount(appUser.Email);
                    // Go back to login page
                    return RedirectToAction(nameof(ConfirmAccountEmailSent));
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(appUser);
        }
        public IActionResult ConfirmAccount()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmAccount(string email)
        {
            // Lookup user
            ApplicationUser appUser = await userManager.FindByEmailAsync(email);

            // Skip sending email if user doesn't exist, but fail silently.
            if (appUser != null)
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
            }

            // Forward user to page that they should check their email
            return RedirectToAction(nameof(ConfirmAccountEmailSent));
        }
        public IActionResult ConfirmAccountEmailSent()
        {
            return View();
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
        
        // Account - General Usage
        public IActionResult Login()
        {
            return View();
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
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
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
        
        // Application Status
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ApplicationStatus(bool available) {
            // Pick new availability state
            var confirmationState = EventApplication.ConfirmationStateOption.request_sent;
            if (available)
                confirmationState = EventApplication.ConfirmationStateOption.unassigned;
            else 
                confirmationState = EventApplication.ConfirmationStateOption.cancelled;

            // Get User info
            var appUser = await userManager.FindByNameAsync(User.Identity.Name);
            var userId = appUser.Id.ToString();

            // Update in DB
            var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.EventApplications[userId].ConfirmationState, confirmationState);
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == this.activeEvent.Id,
                updateDefinition
            );

            // Update in Memory
            this.activeEvent.EventApplications[userId].ConfirmationState = confirmationState;

            return RedirectToAction(nameof(Index));
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
        public async Task<ViewResult> ResetPassword() {
            string userId = Request.Query["UserId"];
            string code = Request.Query["code"];
            var viewModel = new ResetPasswordViewModel();

            //Lookup user
            ApplicationUser appUser = await this.userManager.FindByIdAsync(userId);
            if (appUser != null)
            {
                viewModel = new ResetPasswordViewModel() {
                    Email= appUser.Email,
                    Code = code
                };
            }

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword) {
            string code = resetPassword.Code;
            string email = resetPassword.Email;
            string password = resetPassword.Password;
            string passwordConfirmation = resetPassword.ConfirmPassword;

            // Verify passwords match
            if (password != passwordConfirmation)
            {
                ModelState.AddModelError("input-error", "Password does not match confirmation.");
                return View();
            }

            // Reset the user's password
            ApplicationUser appUser = await this.userManager.FindByEmailAsync(email);
            var result = await this.userManager.ResetPasswordAsync(appUser, code, password);
            if(!result.Succeeded)
            {
                Errors(result);
                return View();
            }

            // If it made it here, success
            return RedirectToAction("ResetPasswordConfirmation");
        }
        public ViewResult ResetPasswordConfirmation() {
            return View();
        }


        // Team
        public ViewResult YourTeam() {
            var team = new Team();

            // Find user information
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;
            var userId = appUser.Id.ToString();

            // Lookup up team with user's id
            if (this.activeEvent.EventAppTeams.ContainsKey(userId))
            {
                var teamId = this.activeEvent.EventAppTeams[userId];
                team = this.activeEvent.Teams[teamId];
            }
            ViewBag.ShowTeamsTime = activeEvent.ShowTeamsTime;
            ViewBag.RegistrationSettings = activeEvent.RegistrationSettings;
            return View(team);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTeamName(Team newTeamInfo){
            // Find user information
            string userName = User.Identity.Name;
            ApplicationUser appUser = await userManager.FindByNameAsync(userName);
            var userId = appUser.Id.ToString();

            // Stop early if team doesn't exist
            if (!this.activeEvent.EventAppTeams.ContainsKey(userId))
                return RedirectToAction(nameof(YourTeam));
            
            // Find team
            var teamId = this.activeEvent.EventAppTeams[userId];
            var team = this.activeEvent.Teams[teamId];

            // Update team name using event controller
            team.Name = newTeamInfo.Name;
            var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
            eventController.UpdateTeam(team);

            return RedirectToAction(nameof(YourTeam));
        }


        // Error Messages
        public IActionResult AccessDenied()
        {
            return View();
        }
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