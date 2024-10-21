namespace BoldReports.Json.Base.Connection
{
    public enum JsonConnectionType
    {
        File,

        WebDataConnector,
    }


    public class JsonConnectionDetails
    {
        private bool isMapping = true;
        public string PreferredTableName { get; set; }

        public JsonConnectionType SourceConnectionType { get; set; }

        public JsonFileConnectionDetails SourceFileConnectionInfo { get; set; }

        public JsonWebConnectionDetails SourceWebConnectionInfo { get; set; }

        public bool IsMapping {
            get { return isMapping; }
            set { isMapping = value; }
        }

        public System.Collections.Generic.List<string> SchemaDetails { get; set; }
    }
}
