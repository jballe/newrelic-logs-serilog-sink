using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestWeb.Models;

namespace TestWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Serilog.Log.Information("Requesting Index action");
            return View();
        }

        public IActionResult Test()
        {
            Serilog.Log.Information($"Requesting Error action at {DateTime.UtcNow:O}");
            throw new InvalidOperationException("Something is wrong");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

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
