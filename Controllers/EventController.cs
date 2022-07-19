using HackathonWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HackathonWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        // Class Fields
        private static HackathonEvent activeEvent;

        // Fields
        private readonly ILogger<EventController> _logger;
        private UserManager<ApplicationUser> userManager;
        private IMongoCollection<HackathonEvent> eventCollection;
        private IMongoCollection<EventApplication> eventApplicationCollection;

        // Constructor
        public EventController(ILogger<EventController> logger, UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _logger = logger;
            
            // Hackathon DBs
            this.userManager = userManager;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");
            this.eventApplicationCollection = database.GetCollection<EventApplication>("EventApplications");

            // Load active hackathon event, if not previously loaded
            if (EventController.activeEvent == null)
            {
                var results = this.eventCollection.Find(h => h.Id == ObjectId.Parse("62d0913c493ff39662d52fba"));
                EventController.activeEvent = results.First();
            }

        }

        // Event Settings
        public IActionResult Index()
        {
            // Get events and applications for current event
            var events = eventCollection.Find(s => true).ToList<HackathonEvent>();
            var activeEventApplications = eventApplicationCollection.Find(s => s.EventId == EventController.activeEvent.Id).ToList<EventApplication>();

            // Add to view's data
            ViewBag.events = events;
            ViewBag.activeEventApplications = activeEventApplications;
            return View();
        }
        public ViewResult CreateHackathonEvent() => View();
        [HttpPost]
        public async Task<IActionResult> CreateHackathonEvent(HackathonEvent hackathonEvent)
        {
            // Set blank registration settings
            hackathonEvent.RegistrationSettings = new RegistrationSettings();
            // Recheck if model is valid
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                try {
                    await eventCollection.InsertOneAsync(hackathonEvent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return RedirectToAction("Index");
        }

        public ViewResult UpdateHackathonEvent(string id) {
            var results = this.eventCollection.Find(h => h.Id == ObjectId.Parse(id));
            var hackathonEvent = results.First();
            return View(hackathonEvent);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateHackathonEvent(string id, HackathonEvent hackathonEvent)
        {
            try
            {
                // Form change set
                var updateDefinition = Builders<HackathonEvent>.Update
                    .Set(p => p.Name, hackathonEvent.Name)
                    .Set(p => p.StartTime, hackathonEvent.StartTime)
                    .Set(p => p.EndTime, hackathonEvent.EndTime);
                // Update in DB
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(id),
                    updateDefinition
                );
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return View(hackathonEvent);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteHackathonEvent(string id)
        {
            try
            { 
                await eventCollection.FindOneAndDeleteAsync(s => s.Id == ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("Index");
        }


        // Applications
        [AllowAnonymous]
        public ViewResult CreateEventApplication() {
            ViewBag.RegistrationSettings = EventController.activeEvent.RegistrationSettings;
            return View();
        }
        [AllowAnonymous]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateEventApplication(EventApplication eventApplication)
        {
            // Set application's associated event to the active event
            eventApplication.EventId = EventController.activeEvent.Id;
            
            // Associated logged in user, or create the user then associate it
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                // Set application's associated user to the logged in user
                string userName = User.Identity.Name;
                ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;
                eventApplication.UserId = appUser.Id;
            }
            else
            {
                // Create account
                var accountController = (AccountController) this.HttpContext.RequestServices.GetService(typeof(AccountController));
                accountController.ControllerContext = this.ControllerContext;
                await accountController.Create(eventApplication.AssociatedUser);
                
                // Return to page if error creating account
                if (accountController.ModelState.ErrorCount > 0)
                {
                    ViewBag.RegistrationSettings = EventController.activeEvent.RegistrationSettings;
                    return View();
                }

                // Set application's associated user to the new account
                string email = eventApplication.AssociatedUser.Email;
                ApplicationUser appUser = userManager.FindByEmailAsync(email).Result;
                eventApplication.UserId = appUser.Id;
            }

            // Revalidated model
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                try {
                    await eventApplicationCollection.InsertOneAsync(eventApplication);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        // Errors
        private void Errors(Task result)
        {
            var e = result.Exception;
            Errors(e);
        }
        private void Errors(Exception e)
        {
            ModelState.AddModelError("", e.Message);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
