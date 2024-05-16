namespace BoldReports.Xml.Base.Connection
{
    public enum WebXmlConnectionType
    {
        GeneralXml
    }

    public class XMLWebConnectionDetails
    {
        public string Url { get; set; }

        public string XMLString { get; set; }

        public WebXmlConnectionType WebXmlConnectionType { get; set; }
    }
}
