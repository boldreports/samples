namespace BlazorAppViewer6.Data
{
    public class BoldReportViewerOptions
    {
        public string? ReportName { get; set; }
        public string? ServiceURL { get; set; }
        public string? DataBaseData{ get; set; }
    }
    class DbDetails
    {
        public string? DbName { get; set; }
        public string? DbType { get; set; }
        public string? Description { get; set; }
    }
}
