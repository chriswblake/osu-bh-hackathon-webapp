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
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;

namespace HackathonWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        // Class Fields
        public static HackathonEvent _activeEvent;

        // Fields
        private IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<EventController> _logger;
        private UserManager<ApplicationUser> userManager;
        private IMongoCollection<HackathonEvent> eventCollection;
        private SmtpClient emailClient;

        // Constructor
        public EventController(IWebHostEnvironment webHostEnvironment, ILogger<EventController> logger, UserManager<ApplicationUser> userManager, IMongoDatabase database, SmtpClient emailClient)
        {
            this.webHostEnvironment = webHostEnvironment;
            _logger = logger;
            
            // Hackathon DBs
            this.userManager = userManager;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");

            // Email Functions
            this.emailClient = emailClient;

            // Fill in any null users
            var appsWithoutUser = this.activeEvent.EventApplications.Values.Where(p=> p.AssociatedUser == null);
            foreach(var eventApp in appsWithoutUser)
            {
                // Try to find the user
                eventApp.AssociatedUser = userManager.FindByIdAsync(eventApp.UserId.ToString()).Result;

                // If user does not exist, give it a blank user
                if (eventApp.AssociatedUser == null)
                    eventApp.AssociatedUser = new ApplicationUser() {
                        UserName = "Unknown",
                        FirstName = "Unknown",
                        LastName = "Unknown"};
            }
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
                    foreach (var eventApplication in activeEvent.EventApplications.Values){
                        eventApplication.AssociatedUser = userManager.FindByIdAsync(eventApplication.UserId.ToString()).Result;
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

        // Methods
        #region Event - Basic Settings
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
                    .Set(p => p.TimeZoneId, hackathonEvent.TimeZoneId)
                    .Set(p => p.IsActive, hackathonEvent.IsActive)
                    .Set(p => p.PrimaryHost.DisplayName, hackathonEvent.PrimaryHost.DisplayName)
                    .Set(p => p.PrimaryHost.Title, hackathonEvent.PrimaryHost.Title)
                    .Set(p => p.PrimaryHost.Organization, hackathonEvent.PrimaryHost.Organization)
                    .Set(p => p.SecondaryHost.DisplayName, hackathonEvent.SecondaryHost.DisplayName)
                    .Set(p => p.SecondaryHost.Title, hackathonEvent.SecondaryHost.Title)
                    .Set(p => p.SecondaryHost.Organization, hackathonEvent.SecondaryHost.Organization)
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
        #endregion

        #region Event - Static Page Content
        [HttpGet]
        public ViewResult UpdateAbout()
        {
            // Set title on edit page
            ViewData["Title"] = "Update About";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/Home";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("index", "about");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdateAbout";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAbout(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("index", "about", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdateAbout");
        }

        [HttpGet]
        public ViewResult UpdateSchedule()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Schedule";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/Home";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("index", "schedule");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdateSchedule";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSchedule(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("index", "schedule", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdateSchedule");
        }
        
        [HttpGet]
        public ViewResult UpdatePrizes()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Prizes";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/home/prizes";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("prizes", "winners");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdatePrizes";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePrizes(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("prizes", "winners", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdatePrizes");
        }

        [HttpGet]
        public ViewResult UpdateRaffles()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Raffles";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/home/prizes";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("prizes", "raffles");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdateRaffles";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRaffles(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("prizes", "raffles", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdateRaffles");
        }

        [HttpGet]
        public ViewResult UpdateSponsorBenefits()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Sponsor Benefits";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/Home/SponsorBenefits";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("sponsors", "benefits");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdateSponsorBenefits";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSponsorBenefits(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("sponsors", "benefits", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdateSponsorBenefits");
        }

        [HttpGet]
        public ViewResult UpdateSponsorSupportExamples()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Sponsor - Ways to Support";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/Home/SponsorBenefits";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("sponsors", "support_examples");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdateSponsorSupportExamples";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSponsorSupportExamples(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("sponsors", "support_examples", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdateSponsorSupportExamples");
        }

        [HttpGet]
        public ViewResult UpdatePrepareSuggestions()
        {
            // Set title on edit page
            ViewData["Title"] = "Update Getting Ready - Suggestions";
            // Specify page this content appears on
            ViewBag.contentPageURL = "/Home/GettingReady";
            // Get html from DB
            ViewBag.htmlContent = GetStaticPageContent("prepare", "suggestions");
            // Specify method to call for saving
            ViewBag.updateEndpointName = "UpdatePrepareSuggestions";
            return View("UpdateStaticPageContent");
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePrepareSuggestions(string htmlContent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB and local memory
                    await UpdateStaticPageContent("prepare", "suggestions", htmlContent);
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            // Return to edit page
            return RedirectToAction("UpdatePrepareSuggestions");
        }

        public string GetStaticPageContent(string pageName, string sectionName)
        {
            // Try to get sections for the page
            var pageSections = new Dictionary<string, string>();
            if (this.activeEvent.StaticPageSections.ContainsKey(pageName))
                pageSections = this.activeEvent.StaticPageSections[pageName];

            // Try to get the section's html content
            var htmlContent = "";
            if (pageSections.ContainsKey(sectionName))
                htmlContent = pageSections[sectionName];

            return htmlContent;

        }
        public async Task<UpdateResult> UpdateStaticPageContent(string pageName, string sectionName, string htmlContent)
        {
            // Update in DB
            var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.StaticPageSections[pageName][sectionName], htmlContent);
            string eventId = activeEvent.Id.ToString();
            var updateResult = await eventCollection.UpdateOneAsync(
                s => s.Id == ObjectId.Parse(eventId),
                updateDefinition
            );

            // Update in Memory
            if (updateResult.IsAcknowledged && updateResult.ModifiedCount == 1)
            {
                if (!this.activeEvent.StaticPageSections.ContainsKey(pageName))
                    this.activeEvent.StaticPageSections[pageName] = new Dictionary<string, string>();
                this.activeEvent.StaticPageSections[pageName][sectionName] = htmlContent;
            }

            return updateResult;
        }
        #endregion

        #region Hacking Toys/Equipment
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
        public IActionResult DownloadEquipmentCSV()
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
        #endregion

        #region  User Applications for Current Event
        public List<EventApplication> GetActiveEventApplications()
        {
           var activeEventApplications = this.activeEvent.EventApplications.Values.ToList();
           return activeEventApplications;
        }
        [Authorize]
        public IActionResult Apply()
        {
            // Redirect to homepage if they try to visit this page outside of the registration period.
		    if ( DateTime.Now.Date < activeEvent.RegistrationOpensTime.Date 
                 || DateTime.Now.Date > activeEvent.RegistrationClosesTime.Date)
            {
                return RedirectToAction("Index", "Home");
            }

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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Apply(EventApplication eventApplication)
        {
            // Set unique id and time for this application
            eventApplication.Id = ObjectId.GenerateNewId();
            eventApplication.CreatedOn = DateTime.Now;
            eventApplication.ConfirmationState = EventApplication.ConfirmationStateOption.unconfirmed;
            
            // Set application's associated user to the logged in user
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;
            eventApplication.UserId = appUser.Id;

            // Revalidate model
            ModelState.Clear();
            TryValidateModel(eventApplication);

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
            // If they are logged in, then they have already confirmed their account. So, don't show that part of the message.
            ViewBag.ShowMessageVerifyAccount = !(User?.Identity?.IsAuthenticated ?? false);
            return View();
        }
        public ViewResult AvailabilityStatus() {
            ViewBag.ActiveEvent = this.activeEvent;
            return View(this.activeEvent.EventApplications);
        }
        [HttpPost]
        public async Task<IActionResult> AvailabilityStatus(Dictionary<string, EventApplication> eventApplications)
        {
            // Create change set
            var update = Builders<HackathonEvent>.Update;
            var updates = new List<UpdateDefinition<HackathonEvent>>();
            foreach(var kvp in eventApplications)
            {
                string key = kvp.Key;
                EventApplication eventApplication = kvp.Value;
                if (activeEvent.EventApplications.ContainsKey(key))
                {
                    var updateDefinition = update.Set(p => p.EventApplications[key].ConfirmationState, eventApplication.ConfirmationState);
                    updates.Add(updateDefinition);
                }
            }

            // Update in DB
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == activeEvent.Id,
                update.Combine(updates)
            );

            // Update in memory
            foreach(var kvp in eventApplications)
            {
                string key = kvp.Key;
                EventApplication eventApplication = kvp.Value;
                if (activeEvent.EventApplications.ContainsKey(key))
                {
                    this.activeEvent.EventApplications[key].ConfirmationState = eventApplication.ConfirmationState;
                }
            }

            return RedirectToAction(nameof(AvailabilityStatus));
        }
        public async Task<IActionResult> RequestAvailabilityConfirmationViaEmail() {

            // Get list of unconfirmed applications
            var unconfirmedApplications = this.activeEvent.EventApplications.Values.Where(p=> p.ConfirmationState == EventApplication.ConfirmationStateOption.unconfirmed);

            // Build emails to send and updats to make to database
            var update = Builders<HackathonEvent>.Update;
            var updates = new List<UpdateDefinition<HackathonEvent>>();
            var emails = new List<MailMessage>();
            foreach(var eventApplication in unconfirmedApplications)
            {
                var appUser = eventApplication.AssociatedUser;
                
                // Generate confirmation email body with token
                string templatePath = Path.Combine(webHostEnvironment.WebRootPath, "email-templates", "ConfirmAvailability.html");
                string msgBodyTemplate = System.IO.File.ReadAllText(templatePath);
                string code = await userManager.GenerateUserTokenAsync(appUser, "Default", "Confirm Availability");
                var callbackUrlAvailable = Url.Action("ConfirmAvailability", "Event", new { userId=appUser.Id, code=code, available=true }, protocol: HttpContext.Request.Scheme);
                var callbackUrlUnavailable = Url.Action("ConfirmAvailability", "Event", new { userId=appUser.Id, code=code, available=false }, protocol: HttpContext.Request.Scheme);
                var urlAccount = Url.Action("Index", "Account", new {}, protocol: HttpContext.Request.Scheme);
                string msgBody = string.Format(msgBodyTemplate, appUser.FirstName, callbackUrlAvailable, callbackUrlUnavailable, urlAccount);

                // Set up email to send
                MailMessage mail = new MailMessage();
                mail.To.Add(appUser.Email);
                mail.From = new MailAddress("hackokstate@gmail.com");
                mail.Subject = "Confirm Availability - HackOKState";
                mail.Body = msgBody;
                mail.IsBodyHtml = true;
                emails.Add(mail);

                // Set up command for datbase update
                updates.Add(update.Set(p => p.EventApplications[eventApplication.UserId.ToString()].ConfirmationState, EventApplication.ConfirmationStateOption.request_sent));
                // Save in local memory
                eventApplication.ConfirmationState = EventApplication.ConfirmationStateOption.request_sent;
            }

            // Update in DB
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == this.activeEvent.Id,
                update.Combine(updates)
            );

            // Send emails
            foreach(var mail in emails)
                await emailClient.SendMailAsync(mail);

            return RedirectToAction(nameof(AvailabilityStatus));
        }
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAvailability(string userId, string code, bool available) {
            // Checks
            bool validToken = false;
            bool validUserId = false;
            
            // Get user
            var appUser = await this.userManager.FindByIdAsync(userId);

            // If user is found check token and application
            if (appUser != null)
                validToken = await userManager.VerifyUserTokenAsync(appUser, "Default", "Confirm Availability", code);
                validUserId = this.activeEvent.EventApplications.ContainsKey(userId);

            // End early if any issues
            if (!validToken || !validUserId)
            {
                ModelState.AddModelError("invalid-token", "The token is either invalid or expired. Please login and visit your account page to confirm availability.");
                return RedirectToAction("Index", "Account");
            }

            // Decide and save availability state
            if (
                this.activeEvent.EventApplications[userId].ConfirmationState == EventApplication.ConfirmationStateOption.request_sent ||
                this.activeEvent.EventApplications[userId].ConfirmationState == EventApplication.ConfirmationStateOption.unassigned ||
                this.activeEvent.EventApplications[userId].ConfirmationState == EventApplication.ConfirmationStateOption.cancelled
            )
            {
                // Pick new availability state
                var confirmationState = EventApplication.ConfirmationStateOption.request_sent;
                if (available)
                    confirmationState = EventApplication.ConfirmationStateOption.unassigned;
                else 
                    confirmationState = EventApplication.ConfirmationStateOption.cancelled;

                // Update in DB
                var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.EventApplications[userId].ConfirmationState, confirmationState);
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == this.activeEvent.Id,
                    updateDefinition
                );

                // Update in Memory
                this.activeEvent.EventApplications[userId].ConfirmationState = confirmationState;
            }

            // Show confirmation page
            return View("AvailabilityConfirmation", available);
        }
        public ViewResult ApplicationsSummary() {
            ViewBag.ActiveEvent = this.activeEvent;
            return View(this.activeEvent.EventApplications);
        }
        #endregion

        #region Team Placement
        public IActionResult AssignTeams()
        {
            // Get events and applications for current event
            ViewBag.EventApplications = this.activeEvent.EventApplications.Values.Where(p=>
                p.ConfirmationState == EventApplication.ConfirmationStateOption.assigned ||
                p.ConfirmationState == EventApplication.ConfirmationStateOption.unassigned ).ToList();
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
                if (teamId != null && this.activeEvent.Teams.ContainsKey(teamId))
                {
                    updates.Add(update.Set(p => p.EventAppTeams[userId], teamId));
                    updates.Add(update.Set(p => p.EventApplications[userId].ConfirmationState, EventApplication.ConfirmationStateOption.assigned));
                }else {
                    updates.Add(update.Unset(p => p.EventAppTeams[userId]));
                    updates.Add(update.Set(p => p.EventApplications[userId].ConfirmationState, EventApplication.ConfirmationStateOption.unassigned));
                }
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
        public async Task<IActionResult> AutoAssignTeams()
        {
            // Automatically create 10 teams and assign up to 5 applications to them.
            Dictionary<string, string> teamAssignments = this.activeEvent.GenerateTeams(10, 5);

            // Create change set
            var update = Builders<HackathonEvent>.Update;
            var updates = new List<UpdateDefinition<HackathonEvent>>();

            // Schedule DB to delete existing teams
            foreach (string teamId in this.activeEvent.Teams.Keys)
                updates.Add(update.Unset(p => p.Teams[teamId]));

            // Create teams
            List<string> teamIds = teamAssignments.Values.Distinct().ToList();
            int teamNameNumber = 0;
            foreach (string teamId in teamIds)
            {
                teamNameNumber +=1;
                Team team = new Team() {
                    Id = ObjectId.Parse(teamId),
                    Name = "Team " + teamNameNumber,
                    ReferenceEvent = this.activeEvent
                };
                // Schedule to update in database
                updates.Add(update.Set(p => p.Teams[teamId], team));
            }

            // Assign applications to teams
            foreach(var teamAssignment in teamAssignments)
            {
                string userId = teamAssignment.Key;
                string teamId = teamAssignment.Value;

                // Assign application to a team
                updates.Add(update.Set(p => p.EventAppTeams[userId], teamId));
                // Mark the application state as assigned
                updates.Add(update.Set(p => p.EventApplications[userId].ConfirmationState, EventApplication.ConfirmationStateOption.assigned));
            }

            // Update in DB
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == activeEvent.Id,
                update.Combine(updates)
            );

            // Clear Active Event, so it is triggered to be refreshed on next request.
            this.activeEvent = null;

            return RedirectToAction("AssignTeams");
        }
        public IActionResult CreateTeam() => View();
        #endregion

        #region Team Info
        public IActionResult Teams() {
            List<Team> teams = this.activeEvent.Teams.Values.ToList();
            return View(teams);
        }
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
        public IActionResult UpdateTeam(string id) {
            Team team = this.activeEvent.Teams.GetValueOrDefault(id);
            return View(team);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTeam(string id, Team team){
            // Check input
            if (id == null || team == null)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            if (!this.activeEvent.Teams.ContainsKey(id))
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            string teamId = id;

            // Update team name
            var updateDefinition = Builders<HackathonEvent>.Update
                .Set(p => p.Teams[teamId].Name, team.Name)
                .Set(p=> p.Teams[teamId].ProjectName, team.ProjectName)
                .Set(p=> p.Teams[teamId].ProjectDescription, team.ProjectDescription)
                .Set(p=> p.Teams[teamId].ProjectVideoURL, team.ProjectVideoURL);
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == this.activeEvent.Id,
                updateDefinition
            );

            // Update in memory
            Team theTeam = this.activeEvent.Teams[teamId];
            theTeam.Name = team.Name;
            theTeam.ProjectName = team.ProjectName;
            theTeam.ProjectDescription = team.ProjectDescription;
            theTeam.ProjectVideoURL = team.ProjectVideoURL;

            return RedirectToAction(nameof(Teams));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeam(string id)
        {
            //Check if team is empty
            Team team = this.activeEvent.Teams.GetValueOrDefault(id);
            if (team == null)
            {
                Errors(new KeyNotFoundException("Unknown Team ID"));
                return RedirectToAction(nameof(Teams));
            }
            else if (team.TeamMembers.Count() > 0)
            {
                Errors(new Exception("Team must not have members."));
                return RedirectToAction(nameof(Teams));
            }

            try
            { 
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Unset(p=> p.Teams[id]);

                // Update in DB
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == activeEvent.Id,
                    updateDefinition
                );

                // Update in Memory
                this.activeEvent.Teams.Remove(id);
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction(nameof(Teams));
        }
        public IActionResult NameTags() {
            List<EventApplication> assignedEventApplications = this.activeEvent.EventApplications.Values.Where(p=> p.ConfirmationState == EventApplication.ConfirmationStateOption.assigned).ToList();
            return View(assignedEventApplications);
        }
        public ViewResult ApplicationResumes()
        {
            // Only provide applications with career info
            ViewBag.EventApplications = this.activeEvent.EventApplications.Values.Where( ea =>
                ea.ResumeUrl != null
                || ea.LinkedInUrl != null
                || ea.WebsiteUrl != null 
            ).ToList();
            return View();
        }
        #endregion

        #region Score Questions
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
        #endregion

        #region Roles (for Scoring)
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
        #endregion

        #region Roles for scoring (assigned to users)
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
        #endregion

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
