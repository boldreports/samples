using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using BoldReports.Json.Base.Schema;

namespace BoldReports.Json.Base
{
    using Newtonsoft.Json.Linq;
    using System.Data;
    using System.Collections.ObjectModel;
    using System.IO;
    using Newtonsoft.Json;
    using BoldReports.Json.Base.Connection;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class JsonDataProvider
    {
        public const string JsonDefaultTableName = "JSON Table";

        public const string AnonymousArray = "Anonymous Array";

        private const string FileStringSplitter = @"\";

        private const string UrlStringSplitter = "/";

        public static JsonTableInfo GetJsonSourceTableWithAllSchemaInfos(JsonConnectionDetails jsonConnectionDetails)
        {
            JsonTableInfo resultTableInfo = null;
            if (jsonConnectionDetails != null)
            {
                KeyValuePair<string, string> jsonString = GetJsonString(jsonConnectionDetails);
                object jsonObject = GetJsonObject(jsonString.Value);
                if (jsonObject != null)
                {
                    resultTableInfo = new JsonTableInfo
                    {
                        TableName = jsonString.Key,
                        SelectedJsonSchemas = GetAllJsonSchemaInfos(jsonConnectionDetails, jsonObject)
                    };
                }
            }

            return resultTableInfo;
        }

        public static JsonConnectionDetails GetJsonConnectionDetails(bool isJsonFile, string jsonstring, string connectionPath)
        {
            return GetJsonConnectionDetails(isJsonFile, jsonstring, connectionPath, null);
        }

        public static JsonConnectionDetails GetJsonConnectionDetails(bool isJsonFile, string jsonstring, string connectionPath, string preferredTableName)
        {
            return GetJsonConnectionDetails(isJsonFile, jsonstring, connectionPath, preferredTableName, false);
        }

        public static JsonConnectionDetails GetJsonConnectionDetails(bool isJsonFile, string jsonstring, string connectionPath, string preferredTableName, bool isGoogleAnalytics)
        {
            WebConnectionType jsonHandlerConnectionType = isGoogleAnalytics ? WebConnectionType.GoogleAnalytics : WebConnectionType.GeneralJson;
            return GetJsonConnectionDetails(isJsonFile, jsonstring, connectionPath, preferredTableName, jsonHandlerConnectionType);
        }

        public static JsonConnectionDetails GetJsonConnectionDetails(bool isJsonFile, string jsonstring, string connectionPath, string preferredTableName, WebConnectionType jsonHandlerConnectionType)
        {
            JsonConnectionDetails jsonConnectionDetails = new JsonConnectionDetails()
            {
                PreferredTableName = preferredTableName
            };

            if (isJsonFile)
            {
                jsonConnectionDetails.SourceConnectionType = JsonConnectionType.File;
                jsonConnectionDetails.SourceFileConnectionInfo = new JsonFileConnectionDetails()
                {
                    Path = connectionPath
                };
            }
            else
            {
                jsonConnectionDetails.SourceConnectionType = JsonConnectionType.WebDataConnector;
                jsonConnectionDetails.SourceWebConnectionInfo = new JsonWebConnectionDetails
                {
                    Url = connectionPath,
                    JsonString = jsonstring,
                    WebConnectionType = jsonHandlerConnectionType
                };
            }

            return jsonConnectionDetails;
        }

        public static object GetValidJsonObject(JsonConnectionDetails jsonConnectionDetails)
        {
            string jsonString = string.Empty;
            switch (jsonConnectionDetails.SourceConnectionType)
            {
                case JsonConnectionType.File:
                    string filePath = jsonConnectionDetails.SourceFileConnectionInfo.Path;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        try
                        {
                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                jsonString = reader.ReadToEnd();
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    break;

                case JsonConnectionType.WebDataConnector:
                    jsonString = jsonConnectionDetails.SourceWebConnectionInfo.JsonString;
                    break;
            }

            return JsonConvert.DeserializeObject(jsonString);
        }

        private static KeyValuePair<string, string> GetJsonString(JsonConnectionDetails jsonConnectionDetails)
        {
            bool hasTableName = !string.IsNullOrWhiteSpace(jsonConnectionDetails.PreferredTableName);
            string tableName = hasTableName ? jsonConnectionDetails.PreferredTableName : JsonDefaultTableName;
            string jsonString = string.Empty;
            switch (jsonConnectionDetails.SourceConnectionType)
            {
                case JsonConnectionType.File:
                    string filePath = jsonConnectionDetails.SourceFileConnectionInfo.Path;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            jsonString = reader.ReadToEnd();
                        }

                        if (!hasTableName)
                        {
                            int index = filePath.LastIndexOf(FileStringSplitter, StringComparison.InvariantCulture);
                            int adjustedIndex = index + FileStringSplitter.Length;
                            if (index > -1 && adjustedIndex < filePath.Length)
                            {
                                string tempTableName = filePath.Substring(adjustedIndex);
                                if (!string.IsNullOrWhiteSpace(tempTableName))
                                {
                                    tableName = tempTableName;
                                }
                            }
                        }
                    }

                    break;

                case JsonConnectionType.WebDataConnector:
                    jsonString = jsonConnectionDetails.SourceWebConnectionInfo.JsonString;
                    if (!hasTableName)
                    {
                        string webUrl = jsonConnectionDetails.SourceWebConnectionInfo.Url;
                        if (!string.IsNullOrWhiteSpace(webUrl))
                        {
                            int index = webUrl.LastIndexOf(UrlStringSplitter, StringComparison.InvariantCulture);
                            int adjustedIndex = index + UrlStringSplitter.Length;
                            if (index > -1 && adjustedIndex < webUrl.Length)
                            {
                                string urlLastName = webUrl.Substring(adjustedIndex);
                                if (!string.IsNullOrWhiteSpace(urlLastName))
                                {
                                    int endIndex = urlLastName.IndexOf('?');
                                    if (endIndex > -1)
                                    {
                                        urlLastName = urlLastName.Substring(0, endIndex);
                                        if (!string.IsNullOrWhiteSpace(urlLastName))
                                        {
                                            tableName = urlLastName;
                                        }
                                    }
                                    else
                                    {
                                        tableName = urlLastName;
                                    }
                                }
                            }
                        }
                    }

                    break;
            }

            return new KeyValuePair<string, string>(tableName, jsonString);
        }

        internal static object GetJsonObject(string jsonString)
        {
            object result = null;
            try
            {
                result = JsonConvert.DeserializeObject(jsonString);
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        private static ObservableCollection<JsonSchemaInfo> GetAllJsonSchemaInfos(JsonConnectionDetails jsonConnectionDetails, object jsonObject)
        {
            ManipulateJsonObject(jsonConnectionDetails, ref jsonObject);
            JToken jToken = jsonObject as JToken;
            if (jToken != null)
            {
                switch (jToken.Type)
                {
                    case JTokenType.Object:

                        //JSON Root Object 
                        JObject actualObject = jToken as JObject;
                        if (actualObject != null)
                        {
                            return JsonSchemaInfoUtilities.GetObjectJsonSchemas(actualObject, jsonConnectionDetails.IsMapping);
                        }

                        break;

                    case JTokenType.Array:

                        //JSON Root array
                        JObject anonymousObject = new JObject { { AnonymousArray, jToken } };
                        ObservableCollection<JsonSchemaInfo> resultJsonSchemas = JsonSchemaInfoUtilities.GetObjectJsonSchemas(anonymousObject, jsonConnectionDetails.IsMapping);
                        if (resultJsonSchemas.Count == 1 && resultJsonSchemas.All(resultJsonSchema => resultJsonSchema.IsAnonymousSchema = resultJsonSchema.HasChild))
                        {
                            return resultJsonSchemas;
                        }

                        break;
                }
            }

            return new ObservableCollection<JsonSchemaInfo>();
        }

        public static ObservableCollection<DataTable> GetJsonTables(JsonConnectionDetails jsonConnectionDetails, ObservableCollection<JsonTableInfo> jsonTableInfoCollection)
        {
            ObservableCollection<DataTable> tables = new ObservableCollection<DataTable>();
            if (jsonConnectionDetails != null && jsonTableInfoCollection != null)
            {
                KeyValuePair<string, string> jsonString = GetJsonString(jsonConnectionDetails);
                object jsonObject = GetJsonObject(jsonString.Value);
                if (jsonObject != null)
                {
                    tables = GeneralJsonTable(jsonTableInfoCollection, jsonObject, jsonConnectionDetails);
                }

            }

            return tables;
        }

        private static bool IsGoogleAnalyticsManagementAPI(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.Contains("/analytics/v3/management/") : false; // NUll condition check for GA URL
        }

        /// <summary>
        /// Get data tables for selected google sheets
        /// </summary>
        /// <param name="jsonTableInfoCollection">TableInfo collection</param>
        /// <param name="jsonObject">JSON Object data</param>
        /// <param name="jsonConnectionDetails">Json connection details</param>
        /// <returns>data table collection</returns>
        private static ObservableCollection<DataTable> GoogleSheetsJsonTable(ObservableCollection<JsonTableInfo> jsonTableInfoCollection, object jsonObject, JsonConnectionDetails jsonConnectionDetails)
        {
            ObservableCollection<DataTable> tables = new ObservableCollection<DataTable>();
            ManipulateJsonObject(jsonConnectionDetails, ref jsonObject);
            JObject JToken = JObject.Parse(jsonConnectionDetails.SourceWebConnectionInfo.JsonString);
            foreach (JsonTableInfo jsonTableInfo in jsonTableInfoCollection)
            {
                DataTable table = GetDataTableGS(jsonConnectionDetails.PreferredTableName, jsonConnectionDetails.SourceWebConnectionInfo.JsonString, jsonTableInfo.SelectedJsonSchemas);
                tables.Add(table);
            }

            return tables;
        }

        /// <summary>
        /// Get data table for selected google sheets
        /// </summary>
        /// <param name="tableName">TableInfo collection</param>
        /// <param name="json">JSON data</param>
        /// <param name="schema">Selected column schema</param>
        /// <returns>Data table for selected sheet</returns>
        public static DataTable GetDataTableGS(string tableName, string json, ObservableCollection<JsonSchemaInfo> schema = null)
        {
            DataTable table = new DataTable(tableName);
            JArray jsonRows = (JArray)JObject.Parse(json)["values"];
            if (jsonRows == null || jsonRows.Count == 0)
            {
                return table;
            }
            JArray rowHeader = (JArray)jsonRows[0];

            for (int j = 0; j < rowHeader.Count(); j++)
            {
                table.Columns.Add(rowHeader[j]?.ToString(), typeof(string));
            }

            for (int i = 1; i <= jsonRows.Count - 1; i++)
            {
                // Add each of the columns into a new row then put that new row into the DataTable
                rowHeader = (JArray)jsonRows[i];
                JToken value = rowHeader.First;
                DataRow row = table.NewRow();
                if (rowHeader.Count > table.Columns.Count)
                {
                    table = AlterDataTableGS(table, rowHeader);
                }
                // Create the new row, put the values into the columns then add the row to the DataTable
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    if (value != null)
                    {
                        row[table.Columns[j]?.ColumnName] = ((JValue)value).Value.ToString();
                        value = value.Next;
                    }
                }
                table.Rows.Add(row);
            }
            if (schema != null)
            {
                table = RemoveUnselectedColumns(schema, table);
            }
            return table;
        }

        /// <summary>
        /// Alter table if need to add a new column 
        /// </summary>
        /// <param name="table">Data table </param>
        /// <param name="jsonData">JSON data</param>
        /// <returns>updated data table with new columns</returns>
        private static DataTable AlterDataTableGS(DataTable table, JArray jsonData)
        {
            for (int l = table.Columns.Count; l < jsonData.Count; l++)
            {
                table.Columns.Add("", typeof(string));
            }
            return table;
        }

        /// <summary>
        /// Alter data table if selection getting changed
        /// </summary>
        /// <param name="schema">Selected Schema info</param>
        /// <param name="table">Data table</param>
        /// <returns>updated data table after schema change</returns>
        private static DataTable RemoveUnselectedColumns(ObservableCollection<JsonSchemaInfo> schema, DataTable table)
        {

            foreach (JsonSchemaInfo jsonSchema in schema)
            {
                if (jsonSchema.SchemaName == table.TableName)
                {
                    List<string> unselectdColumns = GetUnselectedColumnList(table, jsonSchema);
                    foreach (string columnName in unselectdColumns)
                    {
                        table.Columns.Remove(columnName);
                    }
                }
            }
            return table;
        }

        /// <summary>
        ///Get the unselectd column list
        /// </summary>
        /// <param name="schema">Selected Schema info</param>
        /// <param name="table">Data table</param>
        /// <returns>Returns unselected column list</returns>
        private static List<string> GetUnselectedColumnList(DataTable table, JsonSchemaInfo jsonSchema)
        {
            List<string> unselectdColumns = new List<string>();
            for (int j = 0; j < table.Columns.Count; j++)
            {
                bool columnAvail = false;
                foreach (var item in jsonSchema.ChildSchemas)
                {
                    if (item.SchemaName == table.Columns[j]?.ColumnName)
                    {
                        columnAvail = true;
                        break;
                    }
                }
                if (!columnAvail)
                {
                    unselectdColumns.Add(table.Columns[j]?.ColumnName);
                }
            }
            return unselectdColumns;
        }

        private static ObservableCollection<DataTable> GeneralJsonTable(ObservableCollection<JsonTableInfo> jsonTableInfoCollection, object jsonObject, JsonConnectionDetails jsonConnectionDetails)
        {
            ObservableCollection<DataTable> tables = new ObservableCollection<DataTable>();
            bool isMapping = false;
            ManipulateJsonObject(jsonConnectionDetails, ref jsonObject);
            foreach (JsonTableInfo jsonTableInfo in jsonTableInfoCollection)
            {
                DataTable table = GetJsonTable(jsonConnectionDetails, jsonTableInfo.SelectedJsonSchemas, jsonObject);
                if (table != null)
                {
                    isMapping = jsonConnectionDetails.IsMapping;
                    table.TableName = jsonTableInfo.TableName;
                    JsonTabularUtilities.TrimDataColumnNames(table, jsonConnectionDetails.SchemaDetails, isMapping);
                    tables.Add(table);
                }
            }
            return tables;
        }

        private static void ManipulateJsonObject(JsonConnectionDetails jsonConnectionDetails, ref object jsonObject)
        {
            if (jsonConnectionDetails.SourceConnectionType == JsonConnectionType.WebDataConnector &&
                jsonConnectionDetails.SourceWebConnectionInfo.WebConnectionType == WebConnectionType.GoogleSearchConsole)
            {
                JToken jToken = jsonObject as JToken;
                if (jToken != null && jToken.Type == JTokenType.Object)
                {
                    JToken rowsToken = JsonTabularUtilities.GetValueUsingKey(jToken, "rows");
                    if (rowsToken != null && rowsToken.Type == JTokenType.Array)
                    {
                        IList<JToken> rowCollection = (IList<JToken>)rowsToken;
                        foreach (JToken rowToken in rowCollection)
                        {
                            JToken keysToken = JsonTabularUtilities.GetValueUsingKey(rowToken, "keys");
                            if (keysToken != null && keysToken.Type == JTokenType.Array)
                            {
                                JObject keysJObject = new JObject();
                                IList<JToken> valueCollection = (IList<JToken>)keysToken;
                                for (int i = 0; i < valueCollection.Count; i++)
                                {
                                    keysJObject.Add("key_" + (i + 1), valueCollection[i]);
                                }

                                keysToken.Replace(keysJObject);
                            }
                        }
                    }
                }
            }
        }

        private static DataTable GetJsonTable(JsonConnectionDetails jsonConnectionDetails, ObservableCollection<JsonSchemaInfo> selectedJsonSchemaInfos, object jsonObject)
        {
            ObservableCollection<JsonSchemaInfo> tempSelectedJsonSchemaInfos = selectedJsonSchemaInfos != null && selectedJsonSchemaInfos.Count > 0 ? selectedJsonSchemaInfos : GetAllJsonSchemaInfos(jsonConnectionDetails, jsonObject);
            JToken jToken = jsonObject as JToken;
            if (jToken != null && tempSelectedJsonSchemaInfos != null && tempSelectedJsonSchemaInfos.Count > 0)
            {
                return JsonTabularUtilities.GetJsonTable(tempSelectedJsonSchemaInfos, jToken);
            }

            return new DataTable();
        }

        public static void RefreshJsonSchemaInformation(JsonConnectionDetails jsonConnectionDetails, ObservableCollection<JsonTableInfo> jsonTableInfoCollection)
        {
            if (jsonConnectionDetails != null && jsonTableInfoCollection != null)
            {
                JsonTableInfo sourceTableInfo = GetJsonSourceTableWithAllSchemaInfos(jsonConnectionDetails);
                if (sourceTableInfo != null)
                {
                    foreach (JsonTableInfo jsonTableInfo in jsonTableInfoCollection)
                    {
                        jsonTableInfo.SelectedJsonSchemas = RefreshJsonSchemaInformation(sourceTableInfo.SelectedJsonSchemas, jsonTableInfo.SelectedJsonSchemas);
                    }
                }
            }
        }

        public static ObservableCollection<JsonSchemaInfo> RefreshJsonSchemaInformation(ObservableCollection<JsonSchemaInfo> sourceJsonSchemaCollection, ObservableCollection<JsonSchemaInfo> previousSelectedJsonSchemas)
        {
            if (previousSelectedJsonSchemas == null)
            {
                return null;
            }

            ObservableCollection<JsonSchemaInfo> refreshedJsonSchemaCollection = new ObservableCollection<JsonSchemaInfo>();
            foreach (JsonSchemaInfo previousSelectedJsonSchema in previousSelectedJsonSchemas)
            {
                JsonSchemaInfo selectedJsonSchemaInfo = sourceJsonSchemaCollection.FirstOrDefault(sourceJsonSchema => sourceJsonSchema.IsEqualSchema(previousSelectedJsonSchema));
                if (selectedJsonSchemaInfo != null)
                {
                    if (selectedJsonSchemaInfo.HasChild)
                    {
                        RefreshChilds(selectedJsonSchemaInfo.ChildSchemas, previousSelectedJsonSchema.ChildSchemas);
                    }

                    refreshedJsonSchemaCollection.Add(selectedJsonSchemaInfo);
                }
            }

            return refreshedJsonSchemaCollection;
        }

        private static void RefreshChilds(ObservableCollection<JsonSchemaInfo> currentSelectedChilds, ObservableCollection<JsonSchemaInfo> previousSelectedChilds)
        {
            List<JsonSchemaInfo> removeItems = new List<JsonSchemaInfo>();

            foreach (JsonSchemaInfo currentSelectedChild in currentSelectedChilds)
            {
                JsonSchemaInfo tempPreviousSelectedChild = previousSelectedChilds.FirstOrDefault(previousSelectedChild => previousSelectedChild.IsEqualSchema(currentSelectedChild));
                if (tempPreviousSelectedChild == null)
                {
                    removeItems.Add(currentSelectedChild);
                }
                else
                {
                    if (tempPreviousSelectedChild.HasChild)
                    {
                        RefreshChilds(currentSelectedChild.ChildSchemas, tempPreviousSelectedChild.ChildSchemas);
                    }
                }
            }

            removeItems.ForEach(removeItem => currentSelectedChilds.Remove(removeItem));
        }
    }
}
