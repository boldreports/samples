using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoldReports.Json.Base.Schema
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
                this.schemaNames = Regex.Split(this.columnFullName, JsonProperties.GetUniqueSplitter());
                this.columnName = this.schemaNames.Last();
            }
            else
            {
                this.schemaNames = null;
                this.columnName = null;
            }
        }

        internal string ColumnFullName
        {
            get { return this.columnFullName; }
        }

        internal string[] SchemaNames
        {
            get { return this.schemaNames; }
        }

        internal string ColumnName
        {
            get { return this.columnName; }
        }

        internal void PrepareSimplifiedUniqueColumnName()
        {
            if (this.schemaNames != null && this.schemaNames.Length > 1)
            {
                string uniqueKey = string.Empty;
                for (int i = 0; i < this.schemaNames.Length - 1; i++)
                {
                    string name = this.schemaNames[i];
                    uniqueKey = uniqueKey + (uniqueKey.Length == 0 ? name : JsonProperties.Splitter + name);
                }

                this.columnName += " (" + uniqueKey + ")";
            }
        }
    }
}
