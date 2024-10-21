namespace BoldReports.Xml.Base.Connection
{
    public class XMLConnectionDetails
    {
        public string PreferredTableName { get; set; }

        public XMLConnectionType SourceConnectionType { get; set; }

        public XMLFileConnectionDetails SourceFileConnectionInfo { get; set; }

        public XMLWebConnectionDetails SourceWebConnectionInfo { get; set; }

        public System.Collections.Generic.List<string> SchemaDetails { get; set; }
    }

    public enum XMLConnectionType
    {
        File,

        WebDataConnector,
    }
}
