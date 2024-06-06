namespace BoldReports.Xml.Base
{
    public class XMLProperties
    {
        public XMLProperties()
        {

        }
        internal static string GetUniqueSplitter()
        {
            return SpecialDelimiter + Splitter + ((char)127) + SpecialDelimiter;
        }

        internal const char SpecialDelimiter = '\x01';

        internal const string Splitter = "_";
    }
}
