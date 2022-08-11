using HackathonWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace HackathonWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        // Class Fields
        public static HackathonEvent _activeEvent;

        // Fields
        private readonly ILogger<EventController> _logger;
        private UserManager<ApplicationUser> userManager;
        private IMongoCollection<HackathonEvent> eventCollection;

        // Constructor
        public EventController(ILogger<EventController> logger, UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _logger = logger;
            
            // Hackathon DBs
            this.userManager = userManager;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");
        }

        // Properties
        public HackathonEvent activeEvent {
            get {
                // Load active hackathon event, if not previously loaded
                if (EventController._activeEvent == null)
                {
                    // Try to get the event that is set as active
                    var results = this.eventCollection.Find(h => h.IsActive == true);

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
                            EndTime = DateTime.Now,
                            IsActive = true
                        }).Wait();
                        // Retrieve the event
                        results = this.eventCollection.Find(h => true);
                    }

                    // Create active event
                    var activeEvent = results.First();

                    // Set reference event for all teams
                    foreach(var team in activeEvent.Teams.Values)
                    {
                        team.ReferenceEvent = activeEvent;
                    }

                    // Add the users
                    foreach(var team in activeEvent.Teams.Values){
                        foreach(Guid userId in team.EventApplications.Keys){
                            team.TeamMembers[userId] = userManager.FindByIdAsync(userId.ToString()).Result;
                        }
                    }

                    // Set active event for controller
                    EventController._activeEvent = activeEvent;
                }

                return EventController._activeEvent;
            }
            set {
                EventController._activeEvent = value;
            }
        }

        // Event Settings
        public IActionResult Index()
        {
            // Get events and applications for current event
            var events = eventCollection.Find(s => true).ToList<HackathonEvent>();
            var activeEventApplications = this.activeEvent.EventApplications.Values.ToList();

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
                // Update this hackathon
                var updateDefinition = Builders<HackathonEvent>.Update
                    .Set(p => p.Name, hackathonEvent.Name)
                    .Set(p => p.StartTime, hackathonEvent.StartTime)
                    .Set(p => p.EndTime, hackathonEvent.EndTime)
                    .Set(p => p.IsActive, hackathonEvent.IsActive)
                    .Set(p => p.RegistrationOpensTime, hackathonEvent.RegistrationOpensTime)
                    .Set(p => p.EarlyRegistrationClosesTime, hackathonEvent.EarlyRegistrationClosesTime)
                    .Set(p => p.RegistrationClosesTime, hackathonEvent.RegistrationClosesTime)
                    .Set(p => p.RegistrationSettings.MajorOptions, hackathonEvent.RegistrationSettings.MajorOptions)
                    .Set(p => p.RegistrationSettings.TrainingsAcquiredOptions, hackathonEvent.RegistrationSettings.TrainingsAcquiredOptions)
                    .Set(p => p.RegistrationSettings.TShirtSizeOptions, hackathonEvent.RegistrationSettings.TShirtSizeOptions)
                    .Set(p => p.ShowTeamsTime, hackathonEvent.ShowTeamsTime);
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(id),
                    updateDefinition
                );

                // If it was set to active, make all others inactive
                if (hackathonEvent.IsActive)
                {
                    var updateDefinitionInactive = Builders<HackathonEvent>.Update.Set(p => p.IsActive, false);
                    await eventCollection.FindOneAndUpdateAsync(
                        s => s.Id != ObjectId.Parse(id),
                        updateDefinitionInactive
                    );
                }

                // Clear active event so it is refreshed
                this.activeEvent = null;
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
                await eventCollection.FindOneAndDeleteAsync(s => s.Id == ObjectId.Parse(id) && s.IsActive==false);
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("Index");
        }

        // Equipment
        public IActionResult Equipment()
        {
            var equipment = this.activeEvent.Equipment.Values.ToList();
            return View(equipment);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateEquipment(IFormFile csvEquipment)
        {
            // Load CSV file into memory and then reader
            MemoryStream memoryStream = new MemoryStream();
            csvEquipment.OpenReadStream().CopyTo(memoryStream);
            memoryStream.Position = 0;
            StreamReader reader = new StreamReader(memoryStream, System.Text.Encoding.UTF8, true);

            // Read CSV file from reader
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Convert csv lines into hacking equipment objects
                var equipmentList = csv.GetRecords<HackingEquipment>().ToList();

                // Add an id to each object
                foreach(var piece in equipmentList){
                    piece.Id = ObjectId.GenerateNewId();
                }

                // Create change set
                Dictionary<string, HackingEquipment> equipment = equipmentList.ToDictionary(p=> p.Id.ToString(), p=> p);
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Equipment, equipment);

                // Update in DB
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == this.activeEvent.Id,
                    updateDefinition
                );

                // Update in memory
                this.activeEvent.Equipment = equipment;
            }
            return RedirectToAction("Equipment");
        }
        [HttpPost]
        public async Task<IActionResult> DownloadEquipmentCSV()
        {
            // Save to string
            string csvText = "";
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<HackingEquipmentMap>();
                csv.WriteRecords(activeEvent.Equipment.Values);
                csvText = writer.ToString();
            }

            return File(new System.Text.UTF8Encoding().GetBytes(csvText), "text/csv", "hacking-equipment.csv");
        }
        public sealed class HackingEquipmentMap : ClassMap<HackingEquipment>
        {
            public HackingEquipmentMap()
            {
                Map(m => m.Name);
                Map(m => m.Quantity);
                Map(m => m.Unit);
                Map(m => m.Category);
                Map(m => m.UrlMoreInformation);
            }
        }

        // Applications
        public List<EventApplication> GetActiveEventApplications()
        {
           var activeEventApplications = this.activeEvent.EventApplications.Values.ToList();
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

                // If already applied forward to Thank You page
                bool eventAppExists = this.activeEvent.EventApplications.ContainsKey(appUser.Id.ToString());
                if (eventAppExists)
                   return RedirectToAction("ThankYou");
            }

            // Display form to apply
            ViewBag.RegistrationSettings = this.activeEvent.RegistrationSettings;
            return View();
        }
        [AllowAnonymous]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply(EventApplication eventApplication)
        {
            // Set unique id for this application
            eventApplication.Id = ObjectId.GenerateNewId();
            
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
                    ViewBag.RegistrationSettings = this.activeEvent.RegistrationSettings;
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
                    // Create change set
                    var key = eventApplication.UserId.ToString();
                    var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.EventApplications[key], eventApplication);

                    // Update in DB
                    string eventId = activeEvent.Id.ToString();
                    await eventCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(eventId),
                        updateDefinition
                    );

                    // Update in memory
                    this.activeEvent.EventApplications[key] = eventApplication;
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
        
        // Team Placement
        public IActionResult AssignTeams()
        {
            // Get events and applications for current event
            ViewBag.EventApplications = this.activeEvent.EventApplications.Values.ToList();
            ViewBag.Teams = this.activeEvent.Teams;
            ViewBag.EventAppTeams = this.activeEvent.EventAppTeams;
            ViewBag.ActiveEvent = this.activeEvent;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AssignTeams(Dictionary<string,string> teamAssignments)
        {
            var activeEventApplications = this.activeEvent.EventApplications;

            // Create change set
            var update = Builders<HackathonEvent>.Update;
            var updates = new List<UpdateDefinition<HackathonEvent>>();
            foreach(var teamAssignment in teamAssignments)
            {
                string userId = teamAssignment.Key;
                string teamId = teamAssignment.Value;
                var updateDefinition = update.Set(p => p.EventAppTeams[userId], teamId);
                updates.Add(updateDefinition);
            }

            // Update in DB
            string eventId = activeEvent.Id.ToString();
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == ObjectId.Parse(eventId),
                update.Combine(updates)
            );

            // Clear Active Event, so it is triggered to be refreshed on next request.
            this.activeEvent = null;

            return RedirectToAction("AssignTeams");
        }

        public IActionResult CreateTeam() => View();
        [HttpPost]
        public async Task<IActionResult> CreateTeam(Team team)
        {
            try
            {
                // Set Ids
                team.Id = ObjectId.GenerateNewId();

                // Create change set
                string key = team.Id.ToString();
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Teams[key], team);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                this.activeEvent = null;
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("AssignTeams");
        }
        [HttpPost]
        public async void UpdateTeam(Team team){
            // Update team name
            string teamId = team.Id.ToString();
            var updateDefinition = Builders<HackathonEvent>.Update
                .Set(p => p.Teams[teamId].Name, team.Name);
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == this.activeEvent.Id,
                updateDefinition
            );

            // Clear active event so it is refreshed
            this.activeEvent.Teams[teamId].Name = team.Name;
        }
       
        // Score Questions
        public ViewResult ScoreQuestions()
        {
            var scoreQuestions = this.activeEvent.ScoringQuestions.Values.ToList();
            return View(scoreQuestions);
        }
        public IActionResult CreateScoreQuestion() => View();
        [HttpPost]
        public async Task<IActionResult> CreateScoreQuestion(ScoreQuestion scoreQuestion)
        {
            try
            {
                // Set ID for question
                if (scoreQuestion.Id == ObjectId.Empty)
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
                this.activeEvent = null;

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
        public async Task<IActionResult> DeleteScoreQuestion(string id)
        {
            try
            {
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Unset("scoring_questions." + id);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                this.activeEvent = null;
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("ScoreQuestions");
        }
        
        // Scoring Roles
        public ViewResult ScoringRoles()
        {
            var scoringRoles = this.activeEvent.ScoringRoles.Values.ToList();
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View(scoringRoles);
        }
        public IActionResult CreateScoringRole()
        {
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateScoringRole(ScoringRole scoringRole)
        {
            try
            {
                // Set ID if missing
                if (scoringRole.Id == ObjectId.Empty)
                    scoringRole.Id = ObjectId.GenerateNewId();

                // Check for no roles
                if (scoringRole.ScoreQuestionsIds == null)
                    scoringRole.ScoreQuestionsIds = new List<string>();

                // Create change set
                var key = scoringRole.Id.ToString();
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.ScoringRoles[key], scoringRole);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                   s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                this.activeEvent = null;

                // Go back to list
                return RedirectToAction("ScoringRoles");
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return View(scoringRole);
        }
        public ViewResult UpdateScoringRole(string id) {
            var scoringRole = activeEvent.ScoringRoles[id];
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View(scoringRole);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateScoringRole(string id, ScoringRole scoringRole)
        {
            // Set ID so it stays the same
            scoringRole.Id = ObjectId.Parse(id);

            // Forward input to creation method, which is updating the hackathon event.
            return await CreateScoringRole(scoringRole);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteScoringRole(string id)
        {
            try
            {
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Unset("scoring_roles." + id);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                this.activeEvent = null;
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("ScoringRoles");
        }
        
        // User Scoring Roles
        public ViewResult UserScoringRoles()
        {
            ViewBag.AppUsers = this.userManager.Users.ToList();
            ViewBag.ScoringRoles = this.activeEvent.ScoringRoles;
            ViewBag.UserScoringRoles = this.activeEvent.UserScoringRoles;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserScoringRoles(Dictionary<string,string> userScoringRoles)
        {
            try
            {
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.UserScoringRoles, userScoringRoles);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Clear Active Event, so it is triggered to be refreshed on next request.
                this.activeEvent = null;
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction("UserScoringRoles");
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
