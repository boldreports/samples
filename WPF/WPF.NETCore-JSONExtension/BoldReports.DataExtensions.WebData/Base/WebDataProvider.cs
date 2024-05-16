using Newtonsoft.Json.Linq;
using BoldReports.Json.Base.Connection;
using BoldReports.Json.Base.Schema;
using BoldReports.Web.Data.Handler;
using BoldReports.Web.DataProviders;
using BoldReports.Web.DataProviders.Helper;
using BoldReports.Xml.Base;
using BoldReports.Xml.Base.Connection;
using BoldReports.Xml.Base.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using BoldReports.WebDatasource.Base.Model;
using BoldReports.Data;
using System.Text.RegularExpressions;
using BoldReports.Data.WebData;
using BoldReports.Windows.Data;

namespace BoldReports.Base.WebDataSource
{
    public class WebDataProvider
    {
        public MethodType MethodType { get; set; }
        public string Url { get; set; }

        public List<KeyValuePair<string, string>> Parameters { get; set; }
        public List<KeyValuePair<string, string>> Headers { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RawData { get; set; }
        public SSLCertificateInfo SSLCertificateInformation { get; set; }
        public DataFormat DataFormat { get; set; }
        public bool IsConnected { get; set; }
        public HttpResponseHeaders ResponseHeader { get; set; }

        public bool IsOData { get; set; }

        internal const string Splitter = ",";

        internal List<SchemaColumnInfo> ExcludeColumns
        {
            get;
            set;
        }

        public WebDataProvider()
        {
            this.Parameters = new List<KeyValuePair<string, string>>();
            this.Headers = new List<KeyValuePair<string, string>>();
            this.ExcludeColumns = new List<SchemaColumnInfo>();
        }

        public WebRequestBodyType BodyType
        {
            get;
            set;
        }

        public string GetDataFromWebSource()
        {
            string api = ProcessTemplates(HttpUtility.UrlDecode(this.Url));
            var requestUserTimeline = new HttpRequestMessage(this.MethodType == MethodType.Get ? HttpMethod.Get : HttpMethod.Post,
                api);
            if (this.MethodType == MethodType.Post && this.BodyType == WebRequestBodyType.Parameters && this.Parameters != null && this.Parameters.Count > 0)
            {
                SetRequestBodyContent(requestUserTimeline);
            }
            if (this.MethodType == MethodType.Post && this.BodyType == WebRequestBodyType.Raw && !string.IsNullOrEmpty(this.RawData))
            {
                requestUserTimeline.Content = new StringContent(ProcessTemplates(this.RawData), Encoding.UTF8, "application/json");
            }
            if (this.Headers != null && this.Headers.Count > 0)
            {
                //Prepare Headers in case of Shared Key Authentication
                //CheckAndUpdateCustomHeaders();
                this.Headers.ToList().ForEach(i =>
                {
                    if (requestUserTimeline.Headers.Contains(i.Key))
                    {
                        requestUserTimeline.Headers.Remove(i.Key);
                    }
                    requestUserTimeline.Headers.Add(i.Key, ProcessTemplates(i.Value));
                });
            }
            if (this.AuthenticationType == AuthenticationType.BasicHttpAuthentication)
            {
                requestUserTimeline.Headers.Add("Authorization", "Basic " + GetAuthenticatedString(this.UserName, this.Password));
            }
            requestUserTimeline.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(GetHttpRequestMediaType()));

#if !(SyncfusionFramework4_0 || NETCore)
            WebRequestHandler handler = new WebRequestHandler();
            if (this.SSLCertificateInformation != null)
            {
                handler = this.GetClientCertificateValues(this.SSLCertificateInformation);
            }

            using (var httpClient = new CustomHttpClient(handler))
#else
            using (var httpClient = new CustomHttpClient())
#endif
            {
                var responseUserTimeLine = httpClient.SendAsync(requestUserTimeline).Result;

                ResponseHeader = responseUserTimeLine.Headers;

                var jsonString = responseUserTimeLine.Content.ReadAsStringAsync().Result;

                //if (IsExcelMediaType(responseUserTimeLine.Content.Headers.ContentType.MediaType))
                //{
                //    UpdateExcelConnectionParameters(responseUserTimeLine);
                //}
                this.IsConnected = true;

                if ((responseUserTimeLine.Content.Headers.ContentType != null) && (responseUserTimeLine.StatusCode != HttpStatusCode.OK))
                {
                    if (responseUserTimeLine.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(string.Format("Status Code {0} - {1}", (int)HttpStatusCode.NotFound, "The requested URL is not found"));
                    }
                    if (responseUserTimeLine.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception(string.Format("Status Code {0} - {1}", (int)HttpStatusCode.Unauthorized, " Authentication Information is not set or is not valid."));
                    }
                    this.IsConnected = false;
                    throw new Exception(string.Format("Status Code {0} {1} - {2}", (int)responseUserTimeLine.StatusCode, responseUserTimeLine.StatusCode, jsonString));
                }

                return jsonString;
            }
        }

        public List<TableSchemaInfo> GetSchema(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("File is empty");
            }

            if (CheckValidInput(this.DataFormat, result))
            {
                List<TableSchemaInfo> tableSchema = null;
                List<TableSchemaInfo> tableColumns = new List<TableSchemaInfo>();
                switch (this.DataFormat)
                {
                    case DataFormat.Json:
                        JsonConnectionDetails jsonConnectionDetails = new JsonConnectionDetails();
                        jsonConnectionDetails.SourceConnectionType = JsonConnectionType.WebDataConnector;
                        jsonConnectionDetails.IsMapping = false;
                        jsonConnectionDetails.SourceWebConnectionInfo = new JsonWebConnectionDetails
                        {
                            JsonString = result,
                            WebConnectionType = WebConnectionType.GeneralJson,
                            Url = this.Url
                        };

                        JsonTableInfo tableInfo = BoldReports.Json.Base.JsonDataProvider.GetJsonSourceTableWithAllSchemaInfos(jsonConnectionDetails);
                        tableSchema = GetTableSchemaFromJsonStringWithAllSchemaInfos(tableInfo, ref tableColumns);
                        return tableSchema;

                    case DataFormat.Xml:
                        XMLConnectionDetails xmlConnectionDetails = new XMLConnectionDetails();
                        xmlConnectionDetails.SourceConnectionType = XMLConnectionType.WebDataConnector;
                        xmlConnectionDetails.SourceWebConnectionInfo = new XMLWebConnectionDetails
                        {
                            XMLString = result,
                        };

                        var xmlTableInfo = XMLDataProvider.GetXMLSourceTableWithAllSchemaInfos(xmlConnectionDetails);
                        tableSchema = GetTableSchemaFromXmlStringWithAllSchemaInfos(xmlTableInfo, ref tableColumns);
                        return tableSchema;
                }
            }
            return null;
        }

        public object GetData(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("File is empty");
            }

            if (CheckValidInput(this.DataFormat, result))
            {
                List<TableSchemaInfo> tableSchema = null;
                System.Data.DataTable tableData = null;
                List<TableSchemaInfo> tableColumns = new List<TableSchemaInfo>();

                switch (this.DataFormat)
                {
                    case DataFormat.Json:
                        JsonConnectionDetails jsonConnectionDetails = new JsonConnectionDetails();
                        jsonConnectionDetails.SourceConnectionType = JsonConnectionType.WebDataConnector;
                        jsonConnectionDetails.IsMapping = false;
                        jsonConnectionDetails.SourceWebConnectionInfo = new JsonWebConnectionDetails
                        {
                            JsonString = result,
                            WebConnectionType = WebConnectionType.GeneralJson,
                            Url = this.Url
                        };

                        JsonTableInfo tableInfo = BoldReports.Json.Base.JsonDataProvider.GetJsonSourceTableWithAllSchemaInfos(jsonConnectionDetails);
                        tableSchema = GetTableSchemaFromJsonStringWithAllSchemaInfos(tableInfo, ref tableColumns);
                        jsonConnectionDetails.SchemaDetails = tableColumns.Select(t => t.ColumnName).ToList();
                        tableSchema = this.GetExcludeColumnSchema(tableColumns, tableSchema);
                        ObservableCollection < JsonTableInfo> jsonTableInfoCollection = new ObservableCollection<JsonTableInfo>();
                        if (tableSchema.Count > 0)
                        {
                            jsonTableInfoCollection.Add(new JsonTableInfo
                            {
                                TableName = "TestData",
                                SelectedJsonSchemas = JsonHelper.PortSelectedTableToJsonSchemInfo(tableSchema)
                            });
                        }
                        tableData = BoldReports.Json.Base.JsonDataProvider.GetJsonTables(jsonConnectionDetails, jsonTableInfoCollection).FirstOrDefault();
                        return tableData;
                    case DataFormat.Xml:
                        XMLConnectionDetails xmlConnectionDetails = new XMLConnectionDetails();
                        xmlConnectionDetails.SourceConnectionType = XMLConnectionType.WebDataConnector;
                        xmlConnectionDetails.SourceWebConnectionInfo = new XMLWebConnectionDetails
                        {
                            XMLString = result,
                        };

                        var xmlTableInfo = XMLDataProvider.GetXMLSourceTableWithAllSchemaInfos(xmlConnectionDetails);
                        tableSchema = GetTableSchemaFromXmlStringWithAllSchemaInfos(xmlTableInfo, ref tableColumns);
                        xmlConnectionDetails.SchemaDetails = tableColumns.Select(t => t.ColumnName).ToList();
                        tableSchema = this.GetExcludeColumnSchema(tableColumns, tableSchema);

                        ObservableCollection<XMLTableInfo> xmlTableInfoCollection = new ObservableCollection<XMLTableInfo>();
                        if (tableSchema.Count > 0)
                        {
                            xmlTableInfoCollection.Add(new XMLTableInfo
                            {
                                TableName = "TestData",
                                SelectedXMLSchemas = XmlHelper.PortSelectedTableToXmlSchemInfo(tableSchema)
                            });
                        }

                        tableData = XMLDataProvider.GetXMLTables(xmlConnectionDetails, xmlTableInfoCollection).FirstOrDefault();
                        return tableData;
                }
            }
            return null;
        }

        public List<SchemaData> GetColumnSchema(List<TableSchemaInfo> tableSchemaInfos)
        {
            List<SchemaData> _columns = new List<SchemaData>();
            this.AddDataColumns(tableSchemaInfos, _columns);

            foreach (var customName in _columns)
            {
                var tempDuplicateNames = _columns.Where(tempName => tempName.Name != null && tempName.Name.ToLower() == customName.Name.ToLower()).ToArray();
                if (tempDuplicateNames.Length <= 1)
                {
                    continue;
                }

                foreach (var schemaInfo in tempDuplicateNames)
                {
                    this.PrepareSimplifiedUniqueColumnName(schemaInfo);
                }
            }

            return _columns;
        }

        public string GetTableName()
        {
            if (!string.IsNullOrEmpty(this.Url))
            {
                return (new Uri(this.Url).Segments.LastOrDefault() ?? "JSONTABLE").Replace(".", "").Replace("'", "");
            }
            return "Result";
        }

#if !(SyncfusionFramework4_0 || NETCore)
        private WebRequestHandler GetClientCertificateValues(SSLCertificateInfo certificateInformation)
        {
            var handler = new WebRequestHandler();
            X509Certificate2 certificate = string.IsNullOrWhiteSpace(certificateInformation.ClientCertificatePassword) ? new X509Certificate2(certificateInformation.SslCertificateData) : new X509Certificate2(certificateInformation.SslCertificateData, certificateInformation.ClientCertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            handler.ClientCertificates.Add(certificate);
            return handler;
        }
#endif

        private string GetAuthenticatedString(string username, string password)
        {
            string authenticatedString = string.Empty;
            byte[] userCredential = Encoding.UTF8.GetBytes(username + ":" + password);
            authenticatedString = Convert.ToBase64String(userCredential);
            return authenticatedString;
        }

        private string GetHttpRequestMediaType()
        {
            string mediaType = string.Empty;
            if (this.DataFormat == DataFormat.Json)
            {
                mediaType = "application/json";
            }
            else if (this.DataFormat == DataFormat.Csv)
            {
                mediaType = "application/csv";
            }
            else if (this.DataFormat == DataFormat.Xml)
            {
                if (this.IsAtomTypeXml())
                {
                    mediaType = "application/atom+xml";
                }
                else
                {
                    mediaType = "application/xml";
                }
            }
            else if (this.DataFormat == DataFormat.Excel)
            {
                mediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

            return mediaType;
        }

        public bool IsAtomTypeXml()
        {
            if (this.IsOData)
            {
                return true;
            }
            return false;
        }

        private void SetRequestBodyContent(HttpRequestMessage message)
        {
            KeyValuePair<string, string> contentType = GetContentType();

            if (contentType.Key != null && contentType.Value != null)
            {
                SetMessageContentBasedOnContentType(message, contentType.Value.ToLower());
                this.Headers.Remove(contentType);
            }
            else
            {
                SetMessageContentBasedOnContentType(message);
            }
        }

        private KeyValuePair<string, string> GetContentType()
        {
            for (int i = 0; i < this.Headers.ToList().Count; i++)
            {
                if (this.Headers[i].Key.ToLower().Equals("content-type"))
                {
                    return this.Headers[i];
                }
            }

            return new KeyValuePair<string, string>();
        }

        private void SetMessageContentBasedOnContentType(HttpRequestMessage message, string type = "")
        {
            switch (type)
            {
                case "application/x-www-form-urlencoded":
                case "x-www-form-urlencoded":
                    {
                        message.Content = new FormUrlEncodedContent(this.Parameters);
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                        break;
                    }
                default:
                    {
                        message.Content = new StringContent(GetUrlParameterContent(), Encoding.UTF8, "application/json");
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        break;
                    }
            }
        }

        private string ProcessTemplates(string str)
        {
            UrlParser parser = new UrlParser();
            try
            {
                parser.TemplateCount(str);
                parser.DateReplacer();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!string.IsNullOrEmpty(parser.finalDate))
            {
                return parser.userInput;
            }
            else
            {
                return str;
            }
        }

        private string GetUrlParameterContent()
        {
            if (this.BodyType == WebRequestBodyType.Parameters)
            {
                string urlContent = "{";
                int count = 0;
                this.Parameters.ForEach(i =>
                {
                    if (count > 0)
                    {
                        urlContent += ",";
                    }
                    urlContent += "\"" + i.Key + "\":\"" + ProcessTemplates(i.Value) + "\"";
                    count++;
                });
                return urlContent + "}";
            }
            else
            {
                return this.RawData ?? "{}";
            }
        }

        public bool CheckValidInput(DataFormat dataFormat, string result)
        {
            switch (dataFormat)
            {
                case DataFormat.Json:
                    return CheckFileFormatIsJSON(result);
                case DataFormat.Xml:
                    return CheckFileFormatIsXML(result);
                default:
                    return false;
            }
        }

        /// <summary>
        /// To check file format json 
        /// </summary>
        /// <param name="result">result string</param>
        /// <returns>boolean value</returns>
        private bool CheckFileFormatIsJSON(string result)
        {
            try
            {
                JToken.Parse(result);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is Newtonsoft.Json.JsonReaderException)
                {
                    throw new Exception(ex.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// To check file format XML 
        /// </summary>
        /// <param name="result">result string</param>
        /// <returns>boolean value</returns>
        private bool CheckFileFormatIsXML(string result)
        {
            try
            {
                XDocument.Parse(result);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is System.Xml.XmlException)
                {
                    throw new Exception(ex.Message);
                }
                throw;
            }
        }

        internal void PrepareSimplifiedUniqueColumnName(SchemaData data)
        {
            if (data.CustomProperties != null && data.CustomProperties.Count > 0 && data.CustomProperties.ContainsKey("Schema") && data.CustomProperties["Schema"] != null)
            {
                var schemaNames = data.CustomProperties["Schema"].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (schemaNames != null && schemaNames.Length > 0)
                {
                    string uniqueKey = string.Empty;
                    for (int i = 0; i < schemaNames.Length; i++)
                    {
                        string name = schemaNames[i];
                        uniqueKey = uniqueKey + (uniqueKey.Length == 0 ? name : "_" + name);
                    }

                    data.Name += " (" + uniqueKey + ")";
                }
            }
        }

        private void AddDataColumns(List<TableSchemaInfo> tableSchemaInfos, List<SchemaData> _columns)
        {
            foreach (var item in tableSchemaInfos)
            {
                if (!(item.SchemaType == JsonCustomSchemaType.Array || item.SchemaType == JsonCustomSchemaType.Object || item.XmlSchemaType == XMLCustomSchemaType.XmlSchemaArrayType || item.XmlSchemaType == XMLCustomSchemaType.XmlSchemaComplexType) || !item.HasChild)
                {
                    if(item.ColumnName == null)
                    {
                        continue;
                    }
                    SchemaData _colInfo = new SchemaData();
                    _colInfo.Name = item.ColumnName;
                    _colInfo.SchemaType = SchemaTypes.Field;
                    _colInfo.Field = new Column();
                    _colInfo.Field.DataType = item.Type;
                    _colInfo.CustomProperties = new Dictionary<string, object>();
                    _colInfo.CustomProperties.Add("Schema", item.SchemaName);
                    _columns.Add(_colInfo);
                    continue;
                }
                this.AddDataColumns(item.ColumnSchemaInfoCollection, _columns);
            }
        }

        private TableSchemaInfo GetExcludeSchemas(TableSchemaInfo tableColumn, List<string> columns)
        {
            TableSchemaInfo _column = new TableSchemaInfo
            {
                ColumnName = tableColumn.ColumnName,
                DataType = tableColumn.DataType,
                FiniteArraySchemaType = tableColumn.FiniteArraySchemaType,
                InnerArrayCount = tableColumn.InnerArrayCount,
                IsAnonymousSchema = tableColumn.IsAnonymousSchema,
                SchemaType = tableColumn.SchemaType,
                ValueType = tableColumn.ValueType,
                ParentSchemaName = tableColumn.ParentSchemaName,
                Id = tableColumn.Id,
                HasChild = tableColumn.HasChild,
                SchemaName = tableColumn.SchemaName,
                IsMapping = tableColumn.IsMapping,
                XmlSchemaType = tableColumn.XmlSchemaType,
                XmlValueType = tableColumn.XmlValueType,
                Type = tableColumn.Type
            };

            if (tableColumn.SchemaType != JsonCustomSchemaType.Array && tableColumn.SchemaType != JsonCustomSchemaType.Object 
                && tableColumn.XmlSchemaType != XMLCustomSchemaType.XmlSchemaArrayType 
                && tableColumn.XmlSchemaType != XMLCustomSchemaType.XmlSchemaComplexType && columns.Contains(tableColumn.Id))
            {
                return null;
            }

            foreach (var item in tableColumn.ColumnSchemaInfoCollection)
            {
                var schemaItem = this.GetExcludeSchemas(item, columns);
                if(schemaItem != null)
                {
                    _column.ColumnSchemaInfoCollection.Add(schemaItem);
                }
            }

            if (_column.HasChild && _column.ColumnSchemaInfoCollection.Count == 0 && _column.IsAnonymousSchema != true)
            {
                return null;
            }

            return _column;
        }

        private List<TableSchemaInfo> GetExcludeColumnSchema(List<TableSchemaInfo> tableColumns, List<TableSchemaInfo> tableSchema)
        {
            if (tableColumns.Count > 0 && this.ExcludeColumns.Count > 0)
            {
                List<string> nodes = new List<string>();
                List<SchemaColumnInfo> columnInfos = new List<SchemaColumnInfo>();

                foreach (var column in this.ExcludeColumns)
                {
                    var isMatched = false;
                    foreach (var schema in tableColumns)
                    {
                        if (column.ColumnName == schema.ColumnName)
                        {
                            var parentName = schema.SchemaName.Replace(",", "_");
                            if (parentName == column.SchemaName)
                            {
                                nodes.Add(schema.Id);
                                tableColumns.Remove(schema);
                                isMatched = true;
                                break;
                            }
                        }
                    }
                    if (!isMatched)
                    {
                        columnInfos.Add(column);
                    }
                }

                foreach (var column in columnInfos)
                {
                    foreach (var schema in tableColumns)
                    {
                        if (column.ColumnName == schema.ColumnName)
                        {
                            nodes.Add(schema.Id);
                            tableColumns.Remove(schema);
                            break;
                        }
                    }
                }

                List<TableSchemaInfo> tempSchema = new List<TableSchemaInfo>();

                foreach (var item in tableSchema)
                {
                    var schemaItemData = GetExcludeSchemas(item, nodes);
                    if(schemaItemData != null)
                    {
                        tempSchema.Add(GetExcludeSchemas(item, nodes));
                    }
                }
                return tempSchema;
            }
            return tableSchema;
        }

        private List<TableSchemaInfo> GetTableSchemaFromJsonStringWithAllSchemaInfos(JsonTableInfo jsonTableInfo, ref List<TableSchemaInfo> tableColumns)
        {
            JsonTableInfo tableInfo = jsonTableInfo.Clone() as JsonTableInfo;
            List<TableSchemaInfo> tableSchema = new List<TableSchemaInfo>();

            foreach (var item in tableInfo.SelectedJsonSchemas)
            {
                tableSchema.Add(GetJsonSourceSchema(item, "", "", ref tableColumns));
            }
            return tableSchema;
        }

        private TableSchemaInfo GetJsonSourceSchema(JsonSchemaInfo jsonSchema, string parentName, string schemaName, ref List<TableSchemaInfo> tableColumns)
        {
            TableSchemaInfo columnSchema = new TableSchemaInfo
            {
                ColumnName = jsonSchema.SchemaName,
                DataType = jsonSchema.ValueType.ToString(),
                FiniteArraySchemaType = jsonSchema.FiniteArraySchemaType,
                InnerArrayCount = jsonSchema.InnerArrayCount,
                IsAnonymousSchema = jsonSchema.IsAnonymousSchema,
                SchemaType = jsonSchema.SchemaType,
                ValueType = jsonSchema.ValueType,
                ParentSchemaName = string.IsNullOrEmpty(parentName) ? "" : parentName,
                Id = Guid.NewGuid().ToString("N"),
                HasChild = jsonSchema.HasChild,
                SchemaName = schemaName,
                IsMapping = jsonSchema.IsMapping != null ? jsonSchema.IsMapping : false,
                Type = Json.Base.JsonTabularUtilities.GetDataType(jsonSchema.ValueType).ToString()
            };

            if (columnSchema.IsAnonymousSchema != true && (columnSchema.SchemaType == JsonCustomSchemaType.Array || columnSchema.SchemaType == JsonCustomSchemaType.Object))
            {
                schemaName = (string.IsNullOrEmpty(schemaName) ? string.Empty : (schemaName + WebDataProvider.Splitter)) + columnSchema.ColumnName;
            }

            if (columnSchema.SchemaType != JsonCustomSchemaType.Array && columnSchema.SchemaType != JsonCustomSchemaType.Object)
            {
                tableColumns.Add(columnSchema);
            }

            foreach (var item in jsonSchema.ChildSchemas)
            {
                var schemaItem = GetJsonSourceSchema(item, parentName, schemaName, ref tableColumns);
                if (schemaItem != null)
                {
                    columnSchema.ColumnSchemaInfoCollection.Add(schemaItem);
                }
            }

            return columnSchema;
        }

        /// <summary>
        /// Get the xml table schema from Xml data with all the schema infos.
        /// </summary>
        /// <param name="jsonTableInfo"></param>
        /// <returns></returns>
        private List<TableSchemaInfo> GetTableSchemaFromXmlStringWithAllSchemaInfos(XMLTableInfo xmlTableInfo, ref List<TableSchemaInfo> tableColumns)
        {
            XMLTableInfo tableInfo = xmlTableInfo.Clone() as XMLTableInfo;
            List<TableSchemaInfo> tableSchema = new List<TableSchemaInfo>();

            foreach (var item in tableInfo.SelectedXMLSchemas)
            {
                tableSchema.Add(GetXmlSourceSchema(item, "", "", ref tableColumns));
            }
            return tableSchema;
        }

        /// <summary>
        /// Get the schema for the xml data source.
        /// </summary>
        /// <param name="xmlSchema"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        private TableSchemaInfo GetXmlSourceSchema(XMLSchemaInfo xmlSchema, string parentName, string schemaName, ref List<TableSchemaInfo> tableColumns)
        {
            TableSchemaInfo columnSchema = new TableSchemaInfo
            {
                ColumnName = xmlSchema.SchemaName,
                DataType = xmlSchema.ValueType.ToString(),
                IsAnonymousSchema = xmlSchema.IsAnonymousSchema,
                XmlSchemaType = xmlSchema.SchemaType,
                XmlValueType = xmlSchema.ValueType,
                ParentSchemaName = string.IsNullOrEmpty(parentName) ? "" : parentName,
                Id = Guid.NewGuid().ToString("N"),
                HasChild = xmlSchema.HasChild,
                SchemaName = schemaName,
                Type = XMLTabularUtilities.GetDataType(xmlSchema.ValueType).ToString()
            };

            if (columnSchema.IsAnonymousSchema != true && (columnSchema.XmlSchemaType == XMLCustomSchemaType.XmlSchemaArrayType || columnSchema.XmlSchemaType == XMLCustomSchemaType.XmlSchemaComplexType))
            {
                schemaName = (string.IsNullOrEmpty(schemaName) ? string.Empty : (schemaName + WebDataProvider.Splitter)) + columnSchema.ColumnName;
            }

            if (columnSchema.XmlSchemaType != XMLCustomSchemaType.XmlSchemaArrayType && columnSchema.XmlSchemaType != XMLCustomSchemaType.XmlSchemaComplexType)
            {
                tableColumns.Add(columnSchema);
            }

            foreach (var item in xmlSchema.ChildSchemas)
            {
                var schemaItem = GetXmlSourceSchema(item, parentName, schemaName, ref tableColumns);
                if (schemaItem != null)
                {
                    columnSchema.ColumnSchemaInfoCollection.Add(schemaItem);
                }
            }

            return columnSchema;
        }

        public void RenameColumns(System.Data.DataTable _table, List<APIQueryColumn> _columns)
        {
            if (_columns != null && _columns.Count > 0)
            {
                foreach (var item in _columns)
                {
                    if (!string.IsNullOrEmpty(item.AliasName) && _table.Columns.Contains(item.Name))
                    {
                        _table.Columns[item.Name].ColumnName = item.AliasName;
                    }
                }
            }
        }

        public void SetExcludeColumns(List<APIQueryColumn> _columns)
        {
            if (_columns != null && _columns.Count > 0)
            {
                List<string> _excludeColumns = new List<string>();
                foreach (var item in _columns)
                {
                    if (item.IsHidden)
                    {
                        _excludeColumns.Add(item.Name);
                    }
                }
                this.ExcludeColumns = SchemaColumnInfo.GetColumnInfos(_excludeColumns);
            }
        }
    }

    public class SchemaColumnInfo
    {
        public string ColumnName
        {
            get;
            set;
        }

        public string SchemaName
        {
            get;
            set;
        }

        public static List<SchemaColumnInfo> GetColumnInfos(List<string> columns)
        {
            List<SchemaColumnInfo> _infos = new List<SchemaColumnInfo>();

            if (columns != null)
            {
                foreach (var col in columns)
                {
                    SchemaColumnInfo info = new SchemaColumnInfo();
                    info.SchemaName = string.Empty;

                    int start = col.IndexOf("(");
                    if (start == -1)
                    {
                        info.ColumnName = col;
                    }
                    else
                    {
                        info.ColumnName = col.Substring(0, start - 1).TrimEnd();
                        int end = col.IndexOf(")", start);
                        string result = col.Substring(start + 1, end - (start + 1));
                        info.SchemaName = result;
                    }

                    _infos.Add(info);
                }
            }

            return _infos;
        } 
    }
}
