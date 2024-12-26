using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using JSONtoWebAPIViewerSample.Models;

namespace JSONtoWebAPIViewerSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.toolbarSettings = new BoldReports.Models.ReportViewer.ToolbarSettings();
            ViewBag.toolbarSettings.Items = BoldReports.ReportViewerEnums.ToolbarItems.All;
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
