using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HackathonWebApp.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace HackathonWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // Fields
        private RoleManager<ApplicationRole> roleManager;
        private UserManager<ApplicationUser> userManager;
        private IMongoCollection<Sponsor> sponsorCollection;
        private IMongoCollection<Organizer> organizerCollection;
        private EventController eventController;
        private IMongoCollection<HackathonEvent> eventCollection;
        private IMongoCollection<AwardCertificate> awardCertificatesCollection;

        // Constructors
        public AdminController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IMongoDatabase database, EventController eventController)
        {
            // Identity
            this.roleManager = roleManager;
            this.userManager = userManager;

            // Hackathon DBs
            this.sponsorCollection = database.GetCollection<Sponsor>("Sponsor");
            this.organizerCollection = database.GetCollection<Organizer>("Organizer");
            this.eventController = eventController;
            this.eventCollection = database.GetCollection<HackathonEvent>("Events");
            this.awardCertificatesCollection = database.GetCollection<AwardCertificate>("AwardCertificates");
        }

        // Properties
        private HackathonEvent activeEvent {
            get {
                return this.eventController.activeEvent;
            }
        }

        // Index
        public ViewResult Index()
        {
            dynamic model = new ExpandoObject();
            ViewBag.ActiveEvent = this.activeEvent;
            ViewBag.UsersCount = userManager.Users.Count();
            ViewBag.NewUsers = userManager.Users.AsQueryable().Where(u => u.CreatedOn >= activeEvent.RegistrationOpensTime && u.CreatedOn <= activeEvent.RegistrationClosesTime.AddDays(0.99999)).ToList();
            return View(model);
        }

        //Methods - Users
        public ViewResult Users()
        {
            ViewBag.Users = userManager.Users;
            ViewBag.Roles = roleManager.Roles.ToDictionary(r => r.Id, r=> r);
            return View();
        }
        public async Task<IActionResult> UpdateUserRoles(string id)
        {
            ViewBag.User = await userManager.FindByIdAsync(id);
            ViewBag.Roles = roleManager.Roles.ToDictionary(r => r.Id, r=> r);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserRoles(string userId, string[] roleIds)
        {
            // Find user
            var user = await userManager.FindByIdAsync(userId);

            // Get all valid role Ids
            ViewBag.Roles = roleManager.Roles;
            Dictionary<Guid, ApplicationRole> validRoles = roleManager.Roles.ToDictionary(r=> r.Id, r => r);

            // Convert select roles for faster lookup
            HashSet<Guid> newRoleIds = roleIds.Select(r => new Guid(r)).ToHashSet();

            // Add/Remove roles
            foreach (ApplicationRole role in validRoles.Values)
            {
                if (newRoleIds.Contains(role.Id))
                    await userManager.AddToRoleAsync(user, role.Name);
                else
                    await userManager.RemoveFromRoleAsync(user, role.Name);
            }
            return RedirectToAction(nameof(Users));
        }

        // Methods - Roles
        public ViewResult Roles()
        {
            // Retrieve users with a role and group by roleId
            var usersByRoleId = new Dictionary<Guid, List<ApplicationUser>>();
            foreach (var user in userManager.Users.Where(u => u.Roles.Count > 0))
                foreach (var roleId in user.Roles)
                {
                    // Create list if missing
                    usersByRoleId[roleId] = usersByRoleId.GetValueOrDefault(roleId, new List<ApplicationUser>());
                    // Add user to list
                    usersByRoleId[roleId].Add(user);
                }
            ViewBag.UsersByRoleId = usersByRoleId;
            ViewBag.Roles = roleManager.Roles;
            return View();
        }
        public IActionResult CreateRole() => View();
        [HttpPost]
        public async Task<IActionResult> CreateRole([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new ApplicationRole() {Name = name });
                if (result.Succeeded)
                    return RedirectToAction("Roles");
                else
                    Errors(result);
            }
            return View(name);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            // Find role info
            ApplicationRole role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError("", "No role found");
                return RedirectToAction("Roles");
            }

            // Remove role from all users
            var affectedUsers = await userManager.GetUsersInRoleAsync(role.Name);
            foreach (var user in affectedUsers)
                await userManager.RemoveFromRoleAsync(user, role.Name);

            // Delete role
            IdentityResult result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                Errors(result);
            
            return RedirectToAction("Roles");
        }

        // Methods - Sponsor
        public ViewResult Sponsors()
        {
            var sponsors = this.activeEvent.Sponsors.Values.OrderBy(p=> p.DisplayPriority).ToList();
            return View(sponsors);
        }
        public IActionResult CreateSponsor() => View();
        [HttpPost]
        public async Task<IActionResult> CreateSponsor(Sponsor sponsor, IFormFile Logo)
        {
            if (ModelState.IsValid)
            {
                try
                {   // Save logo into string of model
                    if (Logo != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        Logo.OpenReadStream().CopyTo(memoryStream);
                        sponsor.Logo = Convert.ToBase64String(memoryStream.ToArray());
                    }
                    else
                    {
                        sponsor.Logo = "";
                    }

                    // Assign sponsor an ID
                    sponsor.Id = ObjectId.GenerateNewId();

                    // Create change set
                    var key = sponsor.Id.ToString();
                    var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Sponsors[key], sponsor);

                    // Update in DB
                    string eventId = this.activeEvent.Id.ToString();
                    await this.eventCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(eventId),
                        updateDefinition
                    );

                    // Clear Active Event, so it is triggered to be refreshed on next request.
                    this.activeEvent.Sponsors.Add(sponsor.Id.ToString(), sponsor);

                    return RedirectToAction("Sponsors");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(sponsor);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSponsor(string id)
        {
            try
            { 
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Unset(p=> p.Sponsors[id]);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Update in Memory
                this.activeEvent.Sponsors.Remove(id);

            }
            catch (Exception e)
            {
                // Save errors
                Errors(e);
            }
            return RedirectToAction("Sponsors");
        }
        public IActionResult UpdateSponsor(string id)
        {
            Sponsor sponsor = this.activeEvent.Sponsors[id];
            return View(sponsor);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSponsor(string id, Sponsor sponsor, IFormFile NewLogo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Set missing id in model
                    sponsor.Id = ObjectId.Parse(id);

                    // If there is a new logo, overwrite the old one. (convert to string)
                    if (NewLogo != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        NewLogo.OpenReadStream().CopyTo(memoryStream);
                        sponsor.Logo = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    // Create change set
                    var key = sponsor.Id.ToString();
                    var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Sponsors[key], sponsor);

                    // Update in DB
                    string eventId = activeEvent.Id.ToString();
                    await eventCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(eventId),
                        updateDefinition
                    );

                    // Update in Memory
                    this.activeEvent.Sponsors[id] = sponsor;
                    
                    // Return to table view
                    return RedirectToAction("Sponsors");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(sponsor);
        }


        // Methods - Organizers
        public ViewResult Organizers()
        {
            var organizers = this.activeEvent.Organizers.Values.ToList();
            return View(organizers);
        }
        public IActionResult CreateOrganizer() => View();
        [HttpPost]
        public async Task<IActionResult> CreateOrganizer(Organizer organizer, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {   // Save logo into string of model
                    if (ProfileImage != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        ProfileImage.OpenReadStream().CopyTo(memoryStream);
                        organizer.ProfileImage = Convert.ToBase64String(memoryStream.ToArray());
                    }
                    else
                    {
                        organizer.ProfileImage = "";
                    }

                    // Assign sponsor an ID
                    organizer.Id = ObjectId.GenerateNewId();

                    // Create change set
                    var key = organizer.Id.ToString();
                    var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Organizers[key], organizer);

                    // Update in DB
                    string eventId = this.activeEvent.Id.ToString();
                    await this.eventCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(eventId),
                        updateDefinition
                    );

                    // Clear Active Event, so it is triggered to be refreshed on next request.
                    this.activeEvent.Organizers.Add(organizer.Id.ToString(), organizer);

                    return RedirectToAction("Organizers");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(organizer);
        }
        public IActionResult UpdateOrganizer(string id)
        {
            Organizer organizer = this.activeEvent.Organizers[id];
            return View(organizer);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrganizer(string id, Organizer organizer, IFormFile NewProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Set missing id in model
                    organizer.Id = ObjectId.Parse(id);

                    // If there is a new logo, overwrite the old one. (convert to string)
                    if (NewProfileImage != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        NewProfileImage.OpenReadStream().CopyTo(memoryStream);
                        organizer.ProfileImage = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    // Create change set
                    var key = organizer.Id.ToString();
                    var updateDefinition = Builders<HackathonEvent>.Update.Set(p => p.Organizers[key], organizer);

                    // Update in DB
                    string eventId = activeEvent.Id.ToString();
                    await eventCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(eventId),
                        updateDefinition
                    );

                    // Update in Memory
                    this.activeEvent.Organizers[id] = organizer;
                    
                    // Return to table view
                    return RedirectToAction("Organizers");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(organizer);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrganizer(string id)
        {
            try
            { 
                // Create change set
                var updateDefinition = Builders<HackathonEvent>.Update.Unset(p=> p.Organizers[id]);

                // Update in DB
                string eventId = activeEvent.Id.ToString();
                await eventCollection.FindOneAndUpdateAsync(
                    s => s.Id == ObjectId.Parse(eventId),
                    updateDefinition
                );

                // Update in Memory
                this.activeEvent.Organizers.Remove(id);
            }
            catch (Exception e)
            {
                // Save errors
                Errors(e);
            }
            return RedirectToAction("Organizers");
        }

        // Methods - Award Certificates
        public ViewResult AwardCertificates()
        {
            var awards = this.awardCertificatesCollection.Find(p=> true).ToList();
            ViewBag.EventTimeZoneInfo = this.activeEvent.TimeZoneInfo;
            return View(awards);
        }
        public IActionResult CreateAwardCertificate() {
            
            ViewBag.Users = this.userManager.Users.ToDictionary(p=> p.Id.ToString(), p=> p.FirstName+" "+p.LastName+" ("+p.Email+")");
            ViewBag.Events = this.eventCollection.Find(e=>true).ToList().ToDictionary(p=> p.Id.ToString(), p=> p.Name);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAwardCertificate(AwardCertificate awardCertificate)
        {
            // AutoFill User Info
            ApplicationUser appUser = await userManager.FindByIdAsync(awardCertificate.UserId);
            awardCertificate.FirstName = appUser.FirstName;
            awardCertificate.LastName = appUser.LastName;

            // Autofill Award Info
            awardCertificate.CreationTime = DateTime.Now;

            // Autfill Event Info
            awardCertificate.EventName = this.activeEvent.Name;
            awardCertificate.StartTime = this.activeEvent.StartTime;
            awardCertificate.EndTime = this.activeEvent.EndTime;
            awardCertificate.JudgesCount = this.activeEvent.Organizers.Values.Where(p=> p.Role == "Judge").Count();
            awardCertificate.ParticipantsCount = this.activeEvent.EventApplications.Values.Where(p=> p.ConfirmationState == EventApplication.ConfirmationStateOption.assigned).Count();

            // Recheck if model is valid
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                try {
                    await awardCertificatesCollection.InsertOneAsync(awardCertificate);
                }
                catch (Exception e)
                {
                    Errors(e);
                    return View(awardCertificate);
                }
            }
            return RedirectToAction(nameof(AwardCertificates));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAwardCertificate(string id)
        {
            try
            { 
                await awardCertificatesCollection.FindOneAndDeleteAsync(s => s.Id == ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                Errors(e);
            }
            return RedirectToAction(nameof(AwardCertificates));
        }
        public IActionResult UpdateAwardCertificate(string id)
        {
            AwardCertificate award = this.awardCertificatesCollection.Find(p=> p.Id == ObjectId.Parse(id)).FirstOrDefault();
            return View(award);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAwardCertificate(string id, AwardCertificate awardCertificate)
        {
            try
            {
                //Set missing id in model
                awardCertificate.Id = ObjectId.Parse(id);

                // Create change set
                var update = Builders<AwardCertificate>.Update;
                var updates = new List<UpdateDefinition<AwardCertificate>>();

                // Add changes to queue
                updates.Add(update.Set(c => c.FirstName, awardCertificate.FirstName));
                updates.Add(update.Set(c => c.LastName, awardCertificate.LastName));
                updates.Add(update.Set(c => c.Award, awardCertificate.Award));
                updates.Add(update.Set(c => c.FirstSignatureName, awardCertificate.FirstSignatureName));
                updates.Add(update.Set(c => c.FirstSignatureTitle, awardCertificate.FirstSignatureTitle));
                updates.Add(update.Set(c => c.FirstSignatureOrganization, awardCertificate.FirstSignatureOrganization));
                updates.Add(update.Set(c => c.SecondSignatureName, awardCertificate.SecondSignatureName));
                updates.Add(update.Set(c => c.SecondSignatureTitle, awardCertificate.SecondSignatureTitle));
                updates.Add(update.Set(c => c.SecondSignatureOrganization, awardCertificate.SecondSignatureOrganization));

                // Update in DB
                await awardCertificatesCollection.FindOneAndUpdateAsync(
                    s => s.Id == awardCertificate.Id,
                    update.Combine(updates)
                );

            }
            catch (Exception e)
            {
                Errors(e);
            }
            return View(awardCertificate);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeamAwardCertificates(string teamId, int rank)
        {
            // Convert Rank to award option
            var award = AwardCertificate.AwardOption.participant;
            switch (rank)
            {
                case 1:
                    award = AwardCertificate.AwardOption.first_place;
                    break;
                case 2:
                    award = AwardCertificate.AwardOption.second_place;
                    break;
                case 3:
                    award = AwardCertificate.AwardOption.third_place;
                    break;
            }

            // Get Team Members
            var appUsers = this.activeEvent.Teams[teamId].TeamMembers.Values;

            // Modify template for each user and create certificate
            foreach (ApplicationUser appUser in appUsers)
            {
                // Define Certificate
                AwardCertificate awardCertificate = new AwardCertificate() {
                    // Receiver
                    Award = award,
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName,
                    CreationTime = DateTime.Now,
                    // Event Info
                    EventName = this.activeEvent.Name,
                    StartTime = this.activeEvent.StartTime,
                    EndTime = this.activeEvent.EndTime,
                    JudgesCount = this.activeEvent.Organizers.Values.Where(p=> p.Role == "Judge").Count(),
                    ParticipantsCount = this.activeEvent.EventApplications.Values.Where(p=> p.ConfirmationState == EventApplication.ConfirmationStateOption.assigned).Count(),
                    // Signatures
                    FirstSignatureName = this.activeEvent.PrimaryHost.DisplayName,
                    FirstSignatureTitle = this.activeEvent.PrimaryHost.Title,
                    FirstSignatureOrganization = this.activeEvent.PrimaryHost.Organization,
                    SecondSignatureName = this.activeEvent.SecondaryHost.DisplayName,
                    SecondSignatureTitle = this.activeEvent.SecondaryHost.Title,
                    SecondSignatureOrganization = this.activeEvent.SecondaryHost.Organization
                };

                // Save to DB
                try {
                   await awardCertificatesCollection.InsertOneAsync(awardCertificate);
                }
                catch (Exception e)
                {
                   Errors(e);
                }
            }

            return RedirectToAction(nameof(AwardCertificates));
        }
        [HttpPost]
        public async Task<IActionResult> CreateParticipationCertificates()
        {
            foreach (Team team in this.activeEvent.Teams.Values)
            {
                string teamId = team.Id.ToString();
                int rank = 0; // Participation
                await CreateTeamAwardCertificates(teamId, rank);
            }
            return RedirectToAction(nameof(AwardCertificates));
        }

        // Methods - Errors
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
