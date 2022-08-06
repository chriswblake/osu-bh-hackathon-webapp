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
            model.CountRoles = roleManager.Roles.Count();
            model.CountUsers = userManager.Users.Count();
            model.CountSponsors = sponsorCollection.CountDocuments(s => true);
            return View(model);
        }

        //Methods - Users
        public ViewResult Users()
        {
            return View(userManager.Users);
        }


        // Methods - Roles
        public ViewResult Roles()
        {
            return View(roleManager.Roles);
        }
        public IActionResult CreateRole() => View();
        [HttpPost]
        public async Task<IActionResult> CreateRole([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new ApplicationRole() {Name = name });
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            return View(name);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            ApplicationRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");
            return View("Index", roleManager.Roles);
        }
        public async Task<IActionResult> UpdateRole(string id)
        {
            ApplicationRole role = await roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();
            foreach (ApplicationUser user in userManager.Users)
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRole(RoleModification model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    ApplicationUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await UpdateRole(model.RoleId);
        }

        // Methods - Sponsor
        public ViewResult Sponsors()
        {
            var sponsors = this.activeEvent.Sponsors.Values.ToList();
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
        public async Task<IActionResult> UpdateSponsor(string id)
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
            var organizers = organizerCollection.Find(s => true).ToList<Organizer>();
            return View(organizers);
        }
        public IActionResult CreateOrganizer() => View();
        [HttpPost]
        public async Task<IActionResult> CreateOrganizer(Organizer model, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {   // Save logo into string of model
                    if (ProfileImage != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        ProfileImage.OpenReadStream().CopyTo(memoryStream);
                        model.ProfileImage = Convert.ToBase64String(memoryStream.ToArray());
                    }
                    else
                    {
                        model.ProfileImage = "";
                    }

                    // Create the organizer
                    await organizerCollection.InsertOneAsync(model);
                    return RedirectToAction("Organizers");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(model);
        }
        public async Task<IActionResult> UpdateOrganizer(string id)
        {
            var results = await organizerCollection.FindAsync(s => s.Id == ObjectId.Parse(id));
            Organizer organizer = results.FirstOrDefault();
            return View(organizer);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrganizer(string id, Organizer model, IFormFile NewProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Set missing id in model
                    model.Id = ObjectId.Parse(id);

                    // If there is a new logo, overwrite the old one. (convert to string)
                    if (NewProfileImage != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        NewProfileImage.OpenReadStream().CopyTo(memoryStream);
                        model.ProfileImage = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    // Update in database
                    await organizerCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(id),
                        new ObjectUpdateDefinition<Organizer>(model)
                    );
                    
                    // Return to table view
                    return RedirectToAction("Organizers");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrganizer(string id)
        {
            try
            { 
                // Delete the organizer
                await organizerCollection.FindOneAndDeleteAsync(s => s.Id == ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                // Save errors
                Errors(e);
            }
            return RedirectToAction("Organizers");
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
