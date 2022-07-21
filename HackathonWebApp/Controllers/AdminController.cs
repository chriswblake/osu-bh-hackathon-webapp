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

        // Constructors
        public AdminController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IMongoDatabase database)
        {
            // Identity
            this.roleManager = roleManager;
            this.userManager = userManager;

            // Hackathon DBs
            this.sponsorCollection = database.GetCollection<Sponsor>("Sponsor");
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
            var sponsors = sponsorCollection.Find(s => true).ToList<Sponsor>();
            return View(sponsors);
        }
        public IActionResult CreateSponsor() => View();
        [HttpPost]
        public async Task<IActionResult> CreateSponsor(Sponsor model, IFormFile Logo)
        {
            if (ModelState.IsValid)
            {
                try
                {   // Save logo into string of model
                    if (Logo != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        Logo.OpenReadStream().CopyTo(memoryStream);
                        model.Logo = Convert.ToBase64String(memoryStream.ToArray());
                    }
                    else
                    {
                        model.Logo = "";
                    }

                    // Create the sponsor
                    await sponsorCollection.InsertOneAsync(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSponsor(string id)
        {
            try
            { 
                // Delete the sponsor
                await sponsorCollection.FindOneAndDeleteAsync(s => s.Id == ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                // Save errors
                Errors(e);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> UpdateSponsor(string id)
        {
            var results = await sponsorCollection.FindAsync(s => s.Id == ObjectId.Parse(id));
            Sponsor sponsor = results.FirstOrDefault();
            return View(sponsor);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSponsor(string id, Sponsor model, IFormFile NewLogo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Set missing id in model
                    model.Id = ObjectId.Parse(id);

                    // If there is a new logo, overwrite the old one. (convert to string)
                    if (NewLogo != null)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        NewLogo.OpenReadStream().CopyTo(memoryStream);
                        model.Logo = Convert.ToBase64String(memoryStream.ToArray());
                    }

                    // Update in database
                    await sponsorCollection.FindOneAndUpdateAsync(
                        s => s.Id == ObjectId.Parse(id),
                        new ObjectUpdateDefinition<Sponsor>(model)
                    );
                    
                    // Return to table view
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    Errors(e);
                }
            }
            return View(model);
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
