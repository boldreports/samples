using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace ReportViewerBlazor.Data
{
    [Route("api/{controller}/{action}/{id?}")]
    [ApiController]
    public class ReportViewerController : ControllerBase, IReportController
    {
        // Report viewer requires a memory cache to store the information of consecutive client requests and
        // the rendered report viewer in the server.
        private IMemoryCache _cache;

        // IWebHostEnvironment used with sample to get the application data from wwwroot.
        private IWebHostEnvironment _hostingEnvironment;

        public ReportViewerController(IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        //Get action for getting resources from the report
        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        [AllowAnonymous]
        // Method will be called from Report Viewer client to get the image src for Image report item.
        public object GetResource(ReportResource resource)
        {
            return ReportHelper.GetResource(resource, this, _cache);
        }

        // Method will be called to initialize the report information to load the report with ReportHelper for processing.
        [NonAction]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            // Get the root path of the web application.
            string basePath = _hostingEnvironment.WebRootPath;
            // Load the XmlReport.rdl report from the application folder wwwroot\Resources.
            System.IO.FileStream inputStream = new System.IO.FileStream(basePath + @"\Resources\" + reportOption.ReportModel.ReportPath + ".rdl", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            MemoryStream reportStream = new MemoryStream();
            inputStream.CopyTo(reportStream);
            reportStream.Position = 0;
            inputStream.Close();

            // Instantiate a serializer to deserialize the report definition.
            BoldReports.RDL.DOM.ReportSerializer serializer = new BoldReports.RDL.DOM.ReportSerializer();
            BoldReports.RDL.DOM.ReportDefinition reportDefinition = serializer.GetReportDefinition(reportStream);

            // Load the XML data from the NewData.xml file into an XmlDocument.
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(basePath + @"\Resources\NewData.xml");
            var xmlDocString = xmlDoc.InnerXml;
            var xmlDocByteArray = Encoding.ASCII.GetBytes(xmlDocString);
            var xmlDocBase64 = Convert.ToBase64String(xmlDocByteArray);
            // Update the report's data source with the Base64 encoded XML data.
            // You can find the data source and then you can change datasource based on your requirements.
            reportDefinition.DataSources[0].ConnectionProperties.EmbeddedData.Data = xmlDocBase64;

            MemoryStream stream = new MemoryStream();
            serializer.SaveReportDefinition(stream, reportDefinition);
            stream.Position = 0;
            reportOption.ReportModel.Stream = stream;
        }

        // Method will be called when report is loaded internally to start the layout process with ReportHelper.
        [NonAction]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        [HttpPost]
        [AllowAnonymous]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, _cache);
        }

        // Post action to process the report from the server based on json parameters and send the result back to the client.
        [HttpPost]
        public object PostReportAction([FromBody] Dictionary<string, object> jsonArray)
        {
            return ReportHelper.ProcessReport(jsonArray, this, this._cache);
        }
    }
}
