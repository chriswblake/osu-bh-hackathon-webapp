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
    [Authorize]
    public class ScoringController : Controller
    {
        // Class Fields
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
        
        // Submit Score (this is the only area that non-admins can access)
        [Authorize]
        public IActionResult Index () {
            return RedirectToAction("SubmitScore");
        }
        [Authorize]
        public ViewResult SubmitScore () {
            // Get User info
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

            try {
                // Get User's Scoring Role
                var scoringRoleId = this.activeEvent.UserScoringRoles[appUser.Id.ToString()];
                var scoringRole = this.activeEvent.ScoringRoles[scoringRoleId];

                // Get only related questions to this role
                var scoringQuestions = this.activeEvent.ScoringQuestions;
                var roleScoringQuestions = scoringRole.ScoreQuestionsIds.Select(id => scoringQuestions[id]).ToList();

                // Share active team for reference
                ViewBag.RoleScoringQuestions = roleScoringQuestions;
                ViewBag.ActiveTeam = this.activeTeam;
            } catch (Exception e) {
                // Set Empty
                ViewBag.RoleScoringQuestions = null;
                ViewBag.ActiveTeam = null;
                // Provide error information
                this.Errors(e);
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitScore(Dictionary<string,int> scores)
        {
            // Get User info
            string userName = User.Identity.Name;
            ApplicationUser appUser = userManager.FindByNameAsync(userName).Result;

            // Create Project Score object
            var scoringSubmission = new ScoringSubmission() {
                Id = ObjectId.GenerateNewId(),
                TeamId = this.activeTeam.Id.ToString(),
                UserId = appUser.Id.ToString(),
                Scores = scores
            };

            // Create change set
            var key = scoringSubmission.TeamId + ", " + scoringSubmission.UserId;
            var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.ScoringSubmissions[key], scoringSubmission);

            // Update in DB
            string eventId = activeEvent.Id.ToString();
            await eventCollection.FindOneAndUpdateAsync(
                s => s.Id == ObjectId.Parse(eventId),
                updateDefinition
            );

            // Update in memory
            this.activeEvent.ScoringSubmissions[key] = scoringSubmission;

            return RedirectToAction("SubmitScore");
        }

        // Dashboard
        [Authorize(Roles = "Admin")]
        public ViewResult ScoringDashboard () {
            // HackathonEvent does not have a list of teams yet. Using hardcoded example teams for development.
            ViewBag.AllTeams = this.activeEvent.Teams;
            ViewBag.ActiveTeam = this.activeTeam;
            ViewBag.ScoringSubmissions = this.activeEvent.ScoringSubmissions;
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;

            // Put scoring submissions in teams
            foreach (ScoringSubmission scoringSubmission in this.activeEvent.ScoringSubmissions.Values)
            {
                string teamId = scoringSubmission.TeamId;
                Team team = this.activeEvent.Teams[teamId];
                team.ScoringSubmissions[scoringSubmission.UserId.ToString()] = scoringSubmission;
            }

            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTeam(string activeTeamId)
        {
            if (activeTeamId != "null") {
                // Find the actual team using the id
                Team team = this.activeEvent.Teams[activeTeamId];
                // Set this team as the active team for scoring.
                this.activeTeam = team;
            }else {
                // Set the active team to none.
                this.activeTeam = null;
            }
            return RedirectToAction("ScoringDashboard");
        }

        // Score Questions
        [Authorize(Roles = "Admin")]
        public ViewResult ScoreQuestions()
        {
            var scoreQuestions = this.activeEvent.ScoringQuestions.Values.ToList();
            return View(scoreQuestions);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult CreateScoreQuestion() => View();
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ViewResult UpdateScoreQuestion(string id) {
            var scoreQuestion = activeEvent.ScoringQuestions[id];
            return View(scoreQuestion);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateScoreQuestion(string id, ScoreQuestion scoreQuestion)
        {
            // Set ID so it stays the same
            scoreQuestion.Id = ObjectId.Parse(id);

            // Forward input to creation method, which is updating the hackathon event.
            return await CreateScoreQuestion(scoreQuestion);
        }
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ViewResult ScoringRoles()
        {
            var scoringRoles = this.activeEvent.ScoringRoles.Values.ToList();
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View(scoringRoles);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult CreateScoringRole()
        {
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View();
        }
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ViewResult UpdateScoringRole(string id) {
            var scoringRole = activeEvent.ScoringRoles[id];
            ViewBag.ScoringQuestions = this.activeEvent.ScoringQuestions;
            return View(scoringRole);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateScoringRole(string id, ScoringRole scoringRole)
        {
            // Set ID so it stays the same
            scoringRole.Id = ObjectId.Parse(id);

            // Forward input to creation method, which is updating the hackathon event.
            return await CreateScoringRole(scoringRole);
        }
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public ViewResult UserScoringRoles()
        {
            ViewBag.AppUsers = this.userManager.Users.ToList();
            ViewBag.ScoringRoles = this.activeEvent.ScoringRoles;
            ViewBag.UserScoringRoles = this.activeEvent.UserScoringRoles;
            return View();
        }
        [Authorize(Roles = "Admin")]
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
