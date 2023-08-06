using HackathonWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HackathonWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IConfiguration configuration { get; }
        private readonly ILogger<HomeController> _logger;
        private IMongoCollection<Organizer> organizerCollection;
        EventController eventController;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger, IMongoDatabase database, EventController eventController)
        {
            this.configuration = configuration;
            _logger = logger;
            
            // Hackathon DBs
            this.organizerCollection = database.GetCollection<Organizer>("Organizer");
            this.eventController = eventController;
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);

            // Show "results" tab, if event is finished.
            if (DateTime.Now.Date > this.activeEvent.EndTime)
                ViewBag.ShowResultsTab = true;

            // Show "prizes" tab, if content is available.
            if (this.activeEvent.StaticPageSections.ContainsKey("prizes"))
                if (this.activeEvent.StaticPageSections["prizes"].ContainsKey("winners"))
                    ViewBag.ShowPrizesTab = true;

            // Show "Scoring" tab, if questions have been defined.
            if (this.activeEvent.ScoringQuestions.Count > 0)
                ViewBag.ShowScoringTab = true;

            // If google analytics tag provided, to enable google analytics script
            ViewBag.GoogleAnalyticsTag = configuration["GOOGLE_ANALYTICS_TAG"] ?? null;
        }

        // Properties
        private HackathonEvent activeEvent {
            get {
                return this.eventController.activeEvent;
            }
        }


        // Methods
        public IActionResult Index()
        {
            // Check dates to see if registration is allowed
            if (DateTime.Now.Date < this.activeEvent.RegistrationOpensTime.Date)
                ViewBag.RegistrationMode = "pending";
            else if (DateTime.Now.Date >= this.activeEvent.RegistrationOpensTime.Date 
                  && DateTime.Now.Date <= this.activeEvent.RegistrationClosesTime.Date)
                ViewBag.RegistrationMode = "open";
            else if (DateTime.Now.Date > this.activeEvent.RegistrationClosesTime.Date)
                ViewBag.RegistrationMode = "finished";

            // Get Dates
            ViewBag.StartTime = this.activeEvent.StartTime;
            ViewBag.EndTime = this.activeEvent.EndTime;
            ViewBag.RegistrationOpensTime = this.activeEvent.RegistrationOpensTime;
            ViewBag.EarlyRegistrationClosesTime = this.activeEvent.EarlyRegistrationClosesTime;
            ViewBag.RegistrationClosesTime = this.activeEvent.RegistrationClosesTime;

            // Get count of applications
            ViewBag.ApplicationsCount = this.activeEvent.EventApplications.Count();

            // Get Schedule Content
            if (this.activeEvent.StaticPageSections.ContainsKey("index"))
                ViewBag.PageSections = this.activeEvent.StaticPageSections["index"];
            
            return View();
        }
        public IActionResult FAQs()
        {
            return View();
        }
        public IActionResult Prizes()
        {
            if (this.activeEvent.StaticPageSections.ContainsKey("prizes"))
                ViewBag.PageSections = this.activeEvent.StaticPageSections["prizes"];
            return View();
        }
        public IActionResult GettingReady()
        {
            if (this.activeEvent.StaticPageSections.ContainsKey("prepare"))
                ViewBag.PageSections = this.activeEvent.StaticPageSections["prepare"];
            var equipment = this.eventController.activeEvent.Equipment.Values.ToList();
            return View(equipment);
        }
        public IActionResult Selection()
        {
            // Use Event Controller to get all event applications for the active event.
            var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
            var activeEventApplications = eventController.GetActiveEventApplications();

            // If there are very few applicants, inject random data. (so the page doesn't look empty)
            if (activeEventApplications.Count < 5)
            {
                var rand = new Random(9000);
                List<string> MajorOptions = new List<string>() {
                    "aerospace_engineering",
                    "architecture",
                    "business",
                    "chemical_engineering",
                    "civil_engineering",
                    "computer_engineering",
                    "computer_science",
                    "electrical_engineering",
                    "environmental_engineering",
                    "industrial_engineering",
                    "management_information Systems",
                    "mechanical_engineering",
                    "microbiology",
                    "petroleum_engineering"
                };
                List<string> SchoolYearOptions = new List<string>() {
                    "first_year",
                    "second_year",
                    "third_year",
                    "fourth_year",
                    "fifth_year",
                    "six_plus_years",
                    "graduate_student",
                    "phd_student"
                };
                // var activeEventApplications = new List<EventApplication>();
                for (int i = 0; i < 5; i++)
                {   
                    activeEventApplications.Add(new EventApplication() {
                        Major = MajorOptions[rand.Next(0,MajorOptions.Count)],
                        SchoolYear = SchoolYearOptions[rand.Next(0,SchoolYearOptions.Count)],
                        HackathonExperience = rand.Next(0, 6),
                        CodingExperience = rand.Next(0, 6),
                        CommunicationExperience = rand.Next(0, 6),
                        OrganizationExperience = rand.Next(0, 6),
                        DocumentationExperience = rand.Next(0, 6),
                        BusinessExperience = rand.Next(0, 6),
                        CreativityExperience = rand.Next(0, 6)
                    });
                }
            }

            

            return View(activeEventApplications);
        }
        public IActionResult Sponsors()
        {
            var sponsors = this.activeEvent.Sponsors.Values
                .Where(p=> p.IsVisible)
                .OrderBy(p=> p.DisplayPriority)
                .ToList();
            return View(sponsors);
        }
        public IActionResult SponsorBenefits()
        {
            if (this.activeEvent.StaticPageSections.ContainsKey("sponsors"))
                ViewBag.PageSections = this.activeEvent.StaticPageSections["sponsors"];
            return View();
        }
        public IActionResult Team()
        {
            var organizers = this.activeEvent.Organizers.Values.ToList();
            return View(organizers);
        }
        public IActionResult Scoring()
        {
            ViewBag.ScoreQuestions = this.activeEvent.ScoringQuestions.Values.ToList();
            return View();
        }
        public IActionResult Results()
        {
            var allTeams = this.activeEvent.Teams;
            Dictionary<string, Team> orderedTeams = allTeams.OrderByDescending(t=> t.Value.CombinedScore).ToDictionary(kvp=> kvp.Key, kvp=> kvp.Value);
            return View(orderedTeams);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
