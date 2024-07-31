using BoldReports.Web.ReportViewer;
using BoldReports.Writer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json.Nodes;
using BoldReports.Web;
using Newtonsoft.Json;
using JSONtoWebAPIViewerSample.Models;
using ProcessingMode = BoldReports.Web.ReportViewer.ProcessingMode;


namespace JSONtoWebAPIViewerSample.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ReportViewerController : Controller, IReportController
    {
        // Report viewer requires a memory cache to store the information of consecutive client request and
        // have the rendered Report Viewer information in server.
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        // IWebHostEnvironment used with sample to get the application data from wwwroot.
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        private readonly Dictionary<string, object> _jsonArray;

        // Post action to process the report from server based json parameters and send the result back to the client.
        public ReportViewerController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        
        // Post action to process the report from server based json parameters and send the result back to the client.
        [HttpPost]
        public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
        {
            return ReportHelper.ProcessReport(jsonArray, this, this._cache);
        }

        // Method will be called to initialize the report information to load the report with ReportHelper for processing.
        [NonAction]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string basePath = _hostingEnvironment.WebRootPath;
    
            // Load the report file
            using (FileStream inputStream = new FileStream(basePath + @"\Resources\" + reportOption.ReportModel.ReportPath, FileMode.Open, FileAccess.Read))
            {
                MemoryStream reportStream = new MemoryStream();
                inputStream.CopyTo(reportStream);
                reportStream.Position = 0;

                // Deserialize the report definition
                BoldReports.RDL.DOM.ReportSerializer reportSerializer = new BoldReports.RDL.DOM.ReportSerializer();
                var reportDefinition = reportSerializer.GetReportDefinition(reportStream);

                // WebAPI connection details
                string _baseUrl = "https://localhost:44372/api/Data";

                foreach (var dataSource in reportDefinition.DataSources)
                {
                    if (dataSource.Name == "DataSource1")
                    {
                        var connectionModel = new
                        {
                            MethodType = "Get",
                            SecurityType = "None",
                            URL = _baseUrl,
                            DataFormat = "JSON",
                            IsCSVFirstRowHeader = true,
                            Separator = "comma",
                            Delimiter = ""
                        };

                        dataSource.ConnectionProperties.ConnectString = JsonConvert.SerializeObject(connectionModel);
                        dataSource.ConnectionProperties.DataProvider = "WebAPI";
                    }
                }

                // Assign the modified report definition directly to the ReportModel
                reportOption.ReportModel.ReportDefinition = reportDefinition;
            }
        }


        // Method will be called when reported is loaded with internally to start to layout process with ReportHelper.
        [NonAction]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
           
        }

        //Get action for getting resources from the report
        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        // Method will be called from Report Viewer client to get the image src for Image report item.
        public object GetResource(ReportResource resource)
        {
            return ReportHelper.GetResource(resource, this, _cache);
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, _cache);
        }
    }
}
