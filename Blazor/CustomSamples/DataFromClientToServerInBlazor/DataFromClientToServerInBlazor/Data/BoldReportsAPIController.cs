using Microsoft.AspNetCore.Mvc;
using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace DataFromClientToServerInBlazor.Data
{
    [Route("api/{controller}/{action}/{id?}")]
    public class BoldReportsAPIController : ControllerBase, IReportController
    {
        // Report viewer requires a memory cache to store the information of consecutive client requests and
        // the rendered report viewer in the server.
        private IMemoryCache _cache;

        // IWebHostEnvironment used with sample to get the application data from wwwroot.
        private IWebHostEnvironment _hostingEnvironment;

        public BoldReportsAPIController(IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;

            _hostingEnvironment = hostingEnvironment;
        }

       

        //Get action for getting resources from the report
        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        // Method will be called from Report Viewer client to get the image src for Image report item.
        public object GetResource(ReportResource resource)
        {
            return ReportHelper.GetResource(resource, this, _cache);
        }

        // Method will be called to initialize the report information to load the report with ReportHelper for processing.
        [NonAction]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string basePath = _hostingEnvironment.WebRootPath;
            // Here, we have loaded the sales-order-detail.rdl report from the application folder wwwroot\Resources. sales-order-detail.rdl should be in the wwwroot\Resources application folder.
            System.IO.FileStream inputStream = new System.IO.FileStream(basePath + @"/resources/" + reportOption.ReportModel.ReportPath + ".rdl", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            MemoryStream reportStream = new MemoryStream();
            inputStream.CopyTo(reportStream);
            reportStream.Position = 0;
            inputStream.Close();
            reportOption.ReportModel.Stream = reportStream;
        }

        // Method will be called when report is loaded internally to start the layout process with ReportHelper.
        [NonAction]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, _cache);
        }
        class DbDetails
        {
            public string? DbName { get; set; }
            public string? DbType { get; set; }
            public string? Description { get; set; }
        }
        private string? jsonString = string.Empty;
        string? DbName = string.Empty;
        string? DbType = string.Empty;
        string? Description = string.Empty;


        [HttpPost]
        public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
        {
            if (jsonArray != null)
            {
                if (jsonArray.ContainsKey("customData"))
                {
                    //Get client side custom data and store in local variable.
                    jsonString = jsonArray["customData"].ToString();
                    DbDetails? dbDetailsObject = JsonConvert.DeserializeObject<DbDetails>(jsonString);
                    DbName = dbDetailsObject.DbName;
                    DbType = dbDetailsObject.DbType;
                    Description = dbDetailsObject.Description;
                }
            }
            return ReportHelper.ProcessReport(jsonArray, this, this._cache);
        }
    }
}
