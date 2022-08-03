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
    public class ScoringController : Controller
    {
        // Class Fields
        private static Dictionary<string, Team> _exampleTeams=null;
        private static Team _activeTeam;

        // Fields
        private readonly ILogger<EventController> _logger;
        private UserManager<ApplicationUser> userManager;
        private IMongoCollection<HackathonEvent> eventCollection;

        // Constructor
        public ScoringController(ILogger<EventController> logger, UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            _logger = logger;
            
            // Hackathon DBs
            this.userManager = userManager;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");
        }

        // Properties
        private Dictionary<string, Team> exampleTeams {
            /// This is a tempory method for development. The current database does not store any information about teams.
            /// Once the DB has team information, it should be pulled dynamically.
            get {
                if (ScoringController._exampleTeams == null)
                {
                    var objectId1 = new ObjectId("62ea9d525e41bf4189147387");
                    var objectId2 = new ObjectId("62ea9d525e41bf4189147386");
                    ScoringController._exampleTeams =  new Dictionary<string, Team> {
                        {objectId1.ToString(), new Team() {
                            Id = objectId1,
                            Name="Team Won",
                            TeamMembers = new Dictionary<Guid, ApplicationUser>() {
                                {new Guid("dee27cff-7c95-4427-90ce-bd041388c1c9"), new ApplicationUser() {FirstName="John", LastName="Smith", Email="john.smith@gmail.com"}},
                                {new Guid("745c87e0-93f7-44e2-92fe-1082256ab086"), new ApplicationUser() {FirstName="Susan", LastName="James", Email="susan@gmail.com"}},
                                {new Guid("37f12881-c4bb-4851-be29-4f133db4e5e8"), new ApplicationUser() {FirstName="David", LastName="Woams", Email="davidw@gmail.com"}}
                            }
                        }},
                        {objectId2.ToString(), new Team() {
                            Id = objectId2,
                            Name="Dos Noches",
                            TeamMembers = new Dictionary<Guid, ApplicationUser>() {
                                {new Guid("8d2f31d2-1081-4e60-b2ee-e2d5378483b0"), new ApplicationUser() {FirstName="Brooks", LastName="Angel", Email="Brangel.smith@gmail.com"}},
                                {new Guid("992d805b-9e1e-46e9-91ad-9475e194afbc"), new ApplicationUser() {FirstName="Carrie", LastName="Dorae", Email="cardo@gmail.com"}},
                                {new Guid("908f7d24-edb6-48f7-88ca-d2feef5b83fe"), new ApplicationUser() {FirstName="William", LastName="Bracky", Email="willb@gmail.com"}}
                            }
                        }},
                    };
                }
                return ScoringController._exampleTeams;
            }
        }
        private HackathonEvent activeEvent {
            get {
                var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
                return eventController.activeEvent;
            }
            set {
                var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
                eventController.activeEvent = value;
            }
        }
        public Team activeTeam {
            get {
                return ScoringController._activeTeam;
            }
            set {
                ScoringController._activeTeam = value;
            }
        }

        // Dashboard
        public ViewResult ScoringDashboard () {
            // HackathonEvent does not have a list of teams yet. Using hardcoded example teams for development.
            ViewBag.AllTeams = this.exampleTeams;
            ViewBag.ActiveTeam = this.activeTeam;
            ViewBag.ScoringSubmissions = this.activeEvent.ScoringSubmissions;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SetActiveTeam(string activeTeamId)
        {
            if (activeTeamId != "null") {
                // Find the actual team using the id
                Team team = this.exampleTeams[activeTeamId];
                // Set this team as the active team for scoring.
                this.activeTeam = team;
            }else {
                // Set the active team to none.
                this.activeTeam = null;
            }
            return RedirectToAction("ScoringDashboard");
        }

        // Submit Score
        public IActionResult Index () {
            return RedirectToAction("SubmitScore");
        }
        [Authorize]
        public ViewResult SubmitScore () {
            // Get User info
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

            // Get User's Scoring Role
            var scoringRoleId = this.activeEvent.UserScoringRoles[appUser.Id.ToString()];
            var scoringRole = this.activeEvent.ScoringRoles[scoringRoleId];

            // Get only related questions to this role
            var scoringQuestions = this.activeEvent.ScoringQuestions;
            var roleScoringQuestions = scoringRole.ScoreQuestionsIds.Select(id => scoringQuestions[id]).ToList();

            // Share active team for reference
            ViewBag.RoleScoringQuestions = roleScoringQuestions;
            ViewBag.ActiveTeam = this.activeTeam;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SubmitScore(Dictionary<string,int> scores)
        {
            // Get User info
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

            // Create Project Score object
            var scoringSubmission = new ScoringSubmission() {
                Id = ObjectId.GenerateNewId(),
                ProjectId = this.activeTeam.Id.ToString(),
                UserId = appUser.Id.ToString(),
                Scores = scores
            };

            // Create change set
            var key = scoringSubmission.ProjectId + ", " + scoringSubmission.UserId;
            var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.ScoringSubmissions[key], scoringSubmission);

            // Update in DB
            string eventId = activeEvent.Id.ToString();
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == ObjectId.Parse(eventId),
                updateDefinition
            );

            // Update in memory
            this.activeEvent.ScoringSubmissions.Add(key, scoringSubmission);

            return RedirectToAction("SubmitScore");
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
