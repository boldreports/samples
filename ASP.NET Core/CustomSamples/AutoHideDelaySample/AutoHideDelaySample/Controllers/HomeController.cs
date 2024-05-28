using AutoHideDelaySample.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AutoHideDelaySample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            ViewBag.toolbarSettings = new BoldReports.Models.ReportViewer.ToolbarSettings();
            ViewBag.toolbarSettings.Items = BoldReports.ReportViewerEnums.ToolbarItems.All
                                               & ~BoldReports.ReportViewerEnums.ToolbarItems.Parameters;
            ViewBag.toolbarSettings.AutoHideDelay = 2; // Customize the delay as per your requirement
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
