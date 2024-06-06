using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BoldReports.Base.WebDataSource;
using BoldReports.Data;
using BoldReports.RDL.DOM;
using BoldReports.Web;
using BoldReports.Web.DataProviders.Helper;
using BoldReports.Windows;
using BoldReports.Windows.Data;

namespace BoldReports.Data.WebData
{
    //[ExtensionConfig(Name = "JSON", Visibility = true)]
    public class JSONExtension : IDataExtension
    {
        private Dictionary<string, object> _customProperties;

        internal bool IsXML
        {
            get;
            set;
        }

        public event ParseExpressionEventHandler ParseExpression;
        public event EvaluateExpressionEventHandler EvaluateExpression;

        //public BoldReports.Data.ConnectionProperties ConnectionProperties
        //{
        //    get;
        //    set;
        //}
        public Windows.Data.ConnectionProperties ConnectionProperties 
        { get ; set ; }
        public Windows.Data.Command Command
        {
            get;
            set;
        }

        public RDL.DOM.DataSource DataSource
        {
            get;
            set;
        }

        public RDL.DOM.DataSet DataSet
        {
            get;
            set;
        }

        public ExtensionHelper Helper
        {
            get;
            set;
        }

        public Dictionary<string, object> CustomProperties
        {
            get
            {
                return this._customProperties;
            }
            set
            {
                if (value != null)
                {
                    this._customProperties = value;
                }
            }
        }

        public bool IsDesignerMode { get; set; }
        //Windows.Data.ConnectionProperties IDataExtension.ConnectionProperties { get ; set ; }
        //Command IDataExtension.Command { get; set; }

        public JSONExtension()
        {
            this.CustomProperties = new Dictionary<string, object>();
            this.CustomProperties.Add("QueryDesignerEnabled", "true");
            this.CustomProperties.Add("QueryFilterEnabled", "false");
            this.CustomProperties.Add("QueryExpressionEnabled", "false");
            this.CustomProperties.Add("QueryJoinerEnabled", "false");
            this.CustomProperties.Add("QueryColumnEdit", "true");
            this.CustomProperties.Add("QueryParameterEnabled", "false");
        }

        event ParseExpressionEventHandler IDataExtension.ParseExpression
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EvaluateExpressionEventHandler IDataExtension.EvaluateExpression
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public string GetCommandText(QueryBuilderDesignInfo designInfo, out string error)
        {
            error = string.Empty;
            if (designInfo != null && designInfo.Tables.Count > 0)
            {
                APIQueryModel queryModel = new APIQueryModel();
                queryModel.Name = designInfo.Tables[0].Name;
                queryModel.Columns = new List<APIQueryColumn>();
                for (var index = 0; index < designInfo.Tables[0].Columns.Count; index++)
                {
                    var hasAliasName = !string.IsNullOrEmpty(designInfo.Tables[0].Columns[index].AliasName);
                    if (!designInfo.Tables[0].Columns[index].IsSelected || hasAliasName)
                    {
                        APIQueryColumn _column = new APIQueryColumn();
                        _column.Name = designInfo.Tables[0].Columns[index].Name;
                        _column.IsHidden = !designInfo.Tables[0].Columns[index].IsSelected;
                        _column.AliasName = hasAliasName ? designInfo.Tables[0].Columns[index].AliasName : string.Empty;
                        queryModel.Columns.Add(_column);
                    }

                }

                return JsonHelper.SerializeObject(queryModel);
            }
            return string.Empty;
        }

        public object GetData(out string error)
        {
            try
            {
                error = string.Empty;

                if (this.IsRDLXMLFormat())
                {
                    return this.GetXMLExtension().GetData(out error);
                }

                if (string.IsNullOrEmpty(this.Command.Text))
                {
                    throw new Exception("Specify the action or table name to execute");
                }

                FileDataModel dataModel = JsonHelper.Deserialize<FileDataModel>(this.ConnectionProperties.ConnectionString);
                WebDataProvider _provider = this.GetDataHelper(dataModel);
                var result = this.GetFileData(dataModel);
                var queryModel = JsonHelper.Deserialize<APIQueryModel>(this.Command.Text);

                _provider.SetExcludeColumns(queryModel.Columns);
                var tableData = _provider.GetData(result) as System.Data.DataTable;
                _provider.RenameColumns(tableData, queryModel.Columns);
                return tableData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
              //  BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return null;
        }

        public SchemaData GetDataSourceSchema(SchemaDataInfo schemaData, out string error)
        {
            try
            {
                error = string.Empty;
                FileDataModel dataModel = JsonHelper.Deserialize<FileDataModel>(this.ConnectionProperties.ConnectionString);
                WebDataProvider _provider = this.GetDataHelper(dataModel);
                var result = this.GetFileData(dataModel);
                var schema = _provider.GetSchema(result);

                SchemaData rootData = new SchemaData();
                rootData.Name = "Schema";
                rootData.SchemaType = SchemaTypes.Database;

                SchemaData tableData = new SchemaData();
                tableData.Name = _provider.GetTableName();
                tableData.SchemaType = SchemaTypes.Table;

                tableData.Data = _provider.GetColumnSchema(schema);

                rootData.Data = new List<SchemaData>();
                rootData.Data.Add(tableData);

                return rootData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
               // BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return null;
        }

        public object GetQuerySchema(out string error)
        {
            try
            {
                error = string.Empty;

                if (string.IsNullOrEmpty(this.Command.Text))
                {
                    throw new Exception("Specify the action or table action name to excute");
                }

                FileDataModel dataModel = JsonHelper.Deserialize<FileDataModel>(this.ConnectionProperties.ConnectionString);
                WebDataProvider _provider = this.GetDataHelper(dataModel);
                var result = this.GetFileData(dataModel);
                var queryModel = JsonHelper.Deserialize<APIQueryModel>(this.Command.Text);

                _provider.SetExcludeColumns(queryModel.Columns);
                var tableData = _provider.GetData(result);
                _provider.RenameColumns(tableData as System.Data.DataTable, queryModel.Columns);
                return tableData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
               // BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return null;
        }

        public bool TestConnection(out string error)
        {
            error = string.Empty;
            if (this.IsRDLXMLFormat())
            {
                return this.GetXMLExtension().TestConnection(out error);
            }
            try
            {
                FileDataModel dataModel = JsonHelper.Deserialize<FileDataModel>(this.ConnectionProperties.ConnectionString);
                WebDataProvider _provider = this.GetDataHelper(dataModel);
                var fileData = this.GetFileData(dataModel);
                return _provider.CheckValidInput(_provider.DataFormat, fileData);
            }
            catch (Exception ex)
            {
                error = ex.Message;
              //  BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return false;
        }

        private WebDataProvider GetDataHelper(FileDataModel dataModel)
        {
            WebDataProvider _provider = new WebDataProvider();
            _provider.DataFormat = this.IsXML ? WebDatasource.Base.Model.DataFormat.Xml : WebDatasource.Base.Model.DataFormat.Json;
            return _provider;
        }

        private string GetFileData(FileDataModel dataModel)
        {
            var fileData = string.Empty;
            if (dataModel.DataMode == "inline")
            {
                fileData = dataModel.Data;
            }
            else if (dataModel.DataMode == "file" && this.ConnectionProperties != null && this.ConnectionProperties.EmbeddedData != null)
            {
                byte[] data = System.Convert.FromBase64String(this.ConnectionProperties.EmbeddedData.Data);
                fileData = Encoding.UTF8.GetString(data);
            }
            else if (dataModel.DataMode == "url")
            {
                if (this.IsValidURL(dataModel.URL))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        fileData = webClient.DownloadString(dataModel.URL);
                        webClient.Dispose();
                    }
                }
                else
                {
                    throw new Exception("URL is invalid. Failure occurred at Scheme validation. Please provide the valid Scheme \"http|https|ftp\"");
                }
            }

            if (string.IsNullOrWhiteSpace(fileData))
            {
                throw new Exception("File is empty");
            }

            return fileData;
        }

        private bool IsValidURL(string url)
        {
            Uri testUri;
            Uri.TryCreate(url, UriKind.Absolute, out testUri);
            if (testUri != null && (testUri.Scheme == Uri.UriSchemeHttp
                || testUri.Scheme == Uri.UriSchemeHttps || testUri.Scheme == Uri.UriSchemeFtp) || !testUri.IsFile)
            {
                return true;
            }
            return false;
        }

        public Dictionary<string, SchemaData> GetTableColumns(List<SchemaDataInfo> schemaData, out string error)
        {
            throw new NotImplementedException();
        }

        public List<Joiner> ValidateAutoJoin(List<string> tableNames, out string error)
        {
            throw new NotImplementedException();
        }

        public RemoveTableData RemoveQueryTable(QueryJoinerInfo joinerInfo, out string error)
        {
            throw new NotImplementedException();
        }

        public List<Joiner> ValidateTableRelations(List<Joiner> joins, out string error)
        {
            throw new NotImplementedException();
        }

        private bool IsRDLXMLFormat()
        {
            if (this.IsXML)
            {
                try
                {
                    if(string.IsNullOrEmpty(this.ConnectionProperties.ConnectionString))
                    {
                        return true;
                    }
                    FileDataModel dataModel = JsonHelper.Deserialize<FileDataModel>(this.ConnectionProperties.ConnectionString);
                    return false;
                }
                catch { }
                return true;
            }
            return false;
        }

        private XmlDataExtension GetXMLExtension()
        {
            XmlDataExtension _xmlExtension = new XmlDataExtension();
            _xmlExtension.Command = this.Command;
            _xmlExtension.ConnectionProperties = this.ConnectionProperties;
            _xmlExtension.CustomProperties = this.CustomProperties;
            _xmlExtension.DataSet = this.DataSet;
            _xmlExtension.DataSource = this.DataSource;
            _xmlExtension.Helper = this.Helper;
            return _xmlExtension;
        }

        public DataTable GetDatabases(out string error)
        {
            throw new NotImplementedException();
        }
    }

    public class FileDataModel
    {
        public string Data { get; set; }

        public string DataMode { get; set; }

        public string URL { get; set; }
    }

}
