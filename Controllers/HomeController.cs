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

        public HomeController(ILogger<HomeController> logger, IMongoClient client)
        {
            _logger = logger;

            // Hackathon DBs
            var database = client.GetDatabase("Hackathon");
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
            return View();
        }

        public IActionResult Sponsors()
        {
            var sponsors = sponsorCollection.Find(s => true).ToList<Sponsor>();
            return View(sponsors);
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
