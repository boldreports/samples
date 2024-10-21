using BoldReports.Json.Base.Connection;
using BoldReports.Json.Base.Schema;
using BoldReports.WebDatasource.Base.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoldReports.Web.DataProviders.Helper
{
	internal class JsonHelper
	{
		internal static ObservableCollection<JsonSchemaInfo> PortSelectedTableToJsonSchemInfo(List<TableSchemaInfo> selectedTableSchema)
		{
			ObservableCollection<JsonSchemaInfo> selectedJsonSchemas = new ObservableCollection<JsonSchemaInfo>();
			selectedTableSchema.ForEach(tableSchema =>
			{
				var schema = new JsonSchemaInfo
				{
					SchemaName = tableSchema.ColumnName,
					FiniteArraySchemaType = tableSchema.FiniteArraySchemaType,
					SchemaType = tableSchema.SchemaType,
					IsAnonymousSchema = tableSchema.IsAnonymousSchema,
					ValueType = tableSchema.ValueType,
					InnerArrayCount = tableSchema.InnerArrayCount,
                    IsMapping = tableSchema?.IsMapping !=null ? tableSchema.IsMapping : false
				};
				var childSchemas = RemoveJsonSchemaFromOriginalSchema(tableSchema.ColumnSchemaInfoCollection);
				foreach (var s in childSchemas)
				{
					schema.ChildSchemas.Add(s);
				}
				selectedJsonSchemas.Add(schema);
			});
			return selectedJsonSchemas;
		}
		private static ObservableCollection<JsonSchemaInfo> RemoveJsonSchemaFromOriginalSchema(List<TableSchemaInfo> selectedTableSchema)
		{
			ObservableCollection<JsonSchemaInfo> selectedJsonSchemas = new ObservableCollection<JsonSchemaInfo>();
			if (selectedTableSchema == null)
			{
				throw new ArgumentNullException();
			}

			foreach (var tableSchema in selectedTableSchema)
			{
				JsonSchemaInfo schema = new JsonSchemaInfo()
				{
					SchemaName = tableSchema.ColumnName,
					FiniteArraySchemaType = tableSchema.FiniteArraySchemaType,
					InnerArrayCount = tableSchema.InnerArrayCount,
					IsAnonymousSchema = tableSchema.IsAnonymousSchema,
					SchemaType = tableSchema.SchemaType,
					ValueType = tableSchema.ValueType,
				};
				foreach (JsonSchemaInfo item in RemoveJsonSchemaFromOriginalSchema(tableSchema.ColumnSchemaInfoCollection))
				{
					schema.ChildSchemas.Add(item);
				}
				selectedJsonSchemas.Add(schema);
			}
			return selectedJsonSchemas;
		}

        /// <summary>
        /// Get Mapping status for column name
        /// </summary>
        /// <param name="SelectedTableSchema">Selected schema information</param>
        /// <returns></returns>
        internal static bool GetMappingStatus(List<TableSchemaInfo> SelectedTableSchema, WebConnectionType webConnectionType)
        {
            if (webConnectionType != WebConnectionType.GeneralJson)
            {
                return false;
            }
            if (SelectedTableSchema != null && SelectedTableSchema?.Count > 0)
            {
                //return SelectedTableSchema.Select(i => i.IsMapping).FirstOrDefault();
                return false;
            }
            return false;
        }

        public static T Deserialize<T>(string jsonstr)
        {
#if NETCore
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonstr);
#else
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Deserialize<T>(jsonstr);
#endif
        }

        internal static string SerializeObject(object value)
        {
#if NETCore
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() });
#else
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(value);
#endif
        }

    }

}
