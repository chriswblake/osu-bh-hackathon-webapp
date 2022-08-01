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
                // Try to get the defined event
                var results = this.eventCollection.Find(h => h.Id == ObjectId.Parse("62d0913c493ff39662d52fba"));

                // If not found, pick the first one
                if (results.CountDocuments() == 0)
                    results = this.eventCollection.Find(h => true);

                // If still not found, create default event
                if (results.CountDocuments() == 0)
                {
                    // Create event
                    CreateHackathonEvent(new HackathonEvent() {
                        Name = "Default Event",
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now
                    }).Wait();
                    // Retrieve the event
                    results = this.eventCollection.Find(h => true);
                }

                // Set active event for controller
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
        public List<EventApplication> GetActiveEventApplications()
        {
           var activeEventApplications = this.eventApplicationCollection.Find(p => p.EventId == EventController.activeEvent.Id).ToList<EventApplication>();
           return activeEventApplications;
        }
        [AllowAnonymous]
        public IActionResult Apply()
        {
            // Skip to "Thank You" page if already applied
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                // Get logged in user
                string userName = User.Identity.Name;
                ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

                // Check if already applied
                var existingApps = eventApplicationCollection.Find(a => a.EventId == EventController.activeEvent.Id && a.UserId == appUser.Id).ToList<EventApplication>();

                // If already applied forward to Thank You page
                if (existingApps.Count > 0)
                   return RedirectToAction("ThankYou");
            }

            // Display form to apply
            ViewBag.RegistrationSettings = EventController.activeEvent.RegistrationSettings;
            return View();
        }
        [AllowAnonymous]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply(EventApplication eventApplication)
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
            return RedirectToAction("ThankYou");
        }
        [AllowAnonymous]
        public ViewResult ThankYou() {
            return View();
        }
        

        // Score Questions
        public ViewResult ScoreQuestions()
        {
            var scoreQuestions = EventController.activeEvent.ScoringQuestions.Values.ToList();
            return View(scoreQuestions);
        }
        public IActionResult CreateScoreQuestion() => View();
        [HttpPost]
        public async Task<IActionResult> CreateScoreQuestion(ScoreQuestion scoreQuestion)
        {
            try
            {
                // Set ID for question
                if (scoreQuestion.Id == null)
                    scoreQuestion.Id = ObjectId.GenerateNewId();

                // Remove empty score options
                scoreQuestion.AnswerOptions = scoreQuestion.AnswerOptions.Where(p=> (p.Value.Description != null) && (p.Value.Description.Trim() != "")).ToDictionary(kv=> kv.Key, kv=> kv.Value);

                // Create change set
                var key = scoreQuestion.Id.ToString();
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.ScoringQuestions[key], scoreQuestion);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                EventController.activeEvent = null;

                // Go back to list
                return RedirectToAction("ScoreQuestions");
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return View(scoreQuestion);
        }
        public ViewResult UpdateScoreQuestion(string id) {
            var scoreQuestion = activeEvent.ScoringQuestions[id];
            return View(scoreQuestion);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateScoreQuestion(string id, ScoreQuestion scoreQuestion)
        {
            // Set ID so it stays the same
            scoreQuestion.Id = ObjectId.Parse(id);

            // Forward input to creation method, which is updating the hackathon event.
            return await CreateScoreQuestion(scoreQuestion);
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
