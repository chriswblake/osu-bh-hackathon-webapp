using HackathonWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<HomeController> _logger;
        private IMongoCollection<Sponsor> sponsorCollection;

        public HomeController(ILogger<HomeController> logger, IMongoDatabase database)
        {
            _logger = logger;
            
            // Hackathon DBs
            this.sponsorCollection = database.GetCollection<Sponsor>("Sponsor");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FAQs()
        {
            return View();
        }

        public IActionResult Selection()
        {
            // Use Event Controller to get all event applications for the active event.
            var eventController = (EventController) this.HttpContext.RequestServices.GetService(typeof(EventController));
            var activeEventApplications = eventController.GetActiveEventApplications();

            return View(activeEventApplications);
        }

        public IActionResult Sponsors()
        {
            var sponsors = sponsorCollection.Find(s => true).ToList<Sponsor>();
            return View(sponsors);
        }
        public IActionResult SponsorBenefits()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
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
