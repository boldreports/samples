namespace BoldReports.Json.Base.Connection
{
    public enum WebConnectionType
    {
        GeneralJson,

        GoogleAnalytics,

        GoogleSearchConsole,

        GoogleSheets
    }

    public class JsonWebConnectionDetails
    {
        public string Url { get; set; }

        public string JsonString { get; set; }

        public WebConnectionType WebConnectionType { get; set; }
    }
}
