using System.Linq;
using System.Text.RegularExpressions;

namespace BoldReports.Xml.Base.Schema
{
    internal class CustomColumnNameInfo
    {
        private readonly string[] schemaNames;
        private readonly string columnFullName;
        private string columnName;

        internal CustomColumnNameInfo(string columnFullName)
        {
            this.columnFullName = columnFullName;
            if (!string.IsNullOrWhiteSpace(this.columnFullName))
            {
                this.schemaNames = Regex.Split(this.columnFullName, XMLProperties.GetUniqueSplitter());
                this.columnName = this.schemaNames.Last();
            }
            else
            {
                this.schemaNames = null;
                this.columnName = null;
            }
        }

        public string[] SchemaName => schemaNames;

        internal string ColumnFullName
        {
            get { return this.columnFullName; }
        }

        internal string[] SchemaNames
        {
            get { return this.SchemaName; }
        }

        internal string ColumnName
        {
            get { return this.columnName; }
        }

        internal void PrepareSimplifiedUniqueColumnName()
        {
            if (this.SchemaName != null && this.SchemaName.Length > 1)
            {
                string uniqueKey = string.Empty;
                for (int i = 0; i < this.SchemaName.Length - 1; i++)
                {
                    string name = this.SchemaName[i];
                    uniqueKey = uniqueKey + (uniqueKey.Length == 0 ? name : XMLProperties.Splitter + name);
                }

                this.columnName += " (" + uniqueKey + ")";
            }
        }
    }
}
