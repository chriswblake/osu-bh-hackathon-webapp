using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HackathonWebApp.Models;


namespace HackathonWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SecuredController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
