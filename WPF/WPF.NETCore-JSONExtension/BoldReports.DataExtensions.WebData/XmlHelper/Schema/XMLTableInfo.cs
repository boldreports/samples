using System;
using System.Collections.ObjectModel;

namespace BoldReports.Xml.Base.Schema
{
    public class XMLTableInfo : ICloneable
    {
        public string TableName { get; set; }

        public ObservableCollection<XMLSchemaInfo> SelectedXMLSchemas { get; set; }

        public object Clone()
        {
            return new XMLTableInfo()
            {
                TableName = this.TableName,
                SelectedXMLSchemas = this.SelectedXMLSchemas
            };
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(this.TableName) ? this.TableName : base.ToString();
        }
    }
}
