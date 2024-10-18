namespace BoldReports.Json.Base
{
    internal class JsonProperties
    {
        private JsonProperties()
        {

        }

        internal static string GetUniqueSplitter()
        {
            return SpecialDelimiter + Splitter + ((char)127) + SpecialDelimiter;            
        }

        internal const char SpecialDelimiter = '\x01';

        internal const string LeftDelimiter = "";

        internal const string RightDelimiter = "";

        internal const string Splitter = "_";
    }
}
