using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoldReports.Json.Base.Schema
{
    public class JsonTableInfo : ICloneable
    {
        public string TableName { get; set; }

        public ObservableCollection<JsonSchemaInfo> SelectedJsonSchemas { get; set; }

        public object Clone()
        {
            JsonTableInfo jsonTableInfo = new JsonTableInfo()
            {
                TableName = this.TableName                
            };

            if (this.SelectedJsonSchemas != null)
            {
                jsonTableInfo.SelectedJsonSchemas = new ObservableCollection<JsonSchemaInfo>();
                foreach (JsonSchemaInfo selectedJsonSchema in this.SelectedJsonSchemas)
                {
                    jsonTableInfo.SelectedJsonSchemas.Add(selectedJsonSchema.Clone() as JsonSchemaInfo);
                }
            }

            return jsonTableInfo;
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(this.TableName) ? this.TableName :  base.ToString();
        }
    }
}
