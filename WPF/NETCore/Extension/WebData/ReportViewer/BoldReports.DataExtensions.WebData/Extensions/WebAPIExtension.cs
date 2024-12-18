using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BoldReports.Base.WebDataSource;
using BoldReports.Data;
using BoldReports.RDL.DOM;
using BoldReports.Web;
using BoldReports.Web.DataProviders.Helper;
using BoldReports.WebDatasource.Base.Model;
using BoldReports.Windows;
using BoldReports.Windows.Data;

namespace BoldReports.Data.WebData
{
    //[ExtensionConfig(Name = "WebAPI", Visibility = true)]
    public class WebAPIExtension : IDataExtension
    {
        private Dictionary<string, object> _customProperties;

        internal bool IsOdata
        {
            get;
            set;
        }

        public event ParseExpressionEventHandler ParseExpression;
        public event EvaluateExpressionEventHandler EvaluateExpression;

        public bool IsDesignerMode
        {
            get;
            set;
        }

        public RDL.DOM.ConnectionProperties ConnectionProperties
        {
            get;
            set;
        }

        public Command Command
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
                if(value != null)
                {
                    this._customProperties = value;
                }
            }
        }

        Windows.Data.ConnectionProperties IDataExtension.ConnectionProperties { get; set; }

        public WebAPIExtension()
        {
            this.CustomProperties = new Dictionary<string, object>();
            this.CustomProperties.Add("QueryDesignerEnabled", "true");
            this.CustomProperties.Add("QueryFilterEnabled", "false");
            this.CustomProperties.Add("QueryExpressionEnabled", "false");
            this.CustomProperties.Add("QueryJoinerEnabled", "false");
            this.CustomProperties.Add("QueryColumnEdit", "true");

            this.ParseExpression += WebAPIExtension_ParseExpression;
            this.EvaluateExpression += WebAPIExtension_EvaluateExpression;
        }

        private void WebAPIExtension_EvaluateExpression(object sender, ExpressionEventArgs e)
        {
            this.ParseDataExpression(e.Action);
            e.Cancel = true;
        }

        private void WebAPIExtension_ParseExpression(object sender, ExpressionEventArgs e)
        {
            this.ParseDataExpression(e.Action);
            e.Cancel = true;
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

                if (string.IsNullOrEmpty(this.Command.Text))
                {
                    throw new Exception("Specify the action or table action name to excute");
                }

                var _provider = this.GetDataHelper();
                var queryModel = JsonHelper.Deserialize<APIQueryModel>(this.Command.Text);

                //if (queryModel.Name.ToLower() != _provider.GetTableName().ToLower())
                //{
                //    throw new Exception("Specify the action or table name is not match");
                //}

                var result = _provider.GetDataFromWebSource();
                _provider.SetExcludeColumns(queryModel.Columns);
                var tableData = _provider.GetData(result) as System.Data.DataTable;
                _provider.RenameColumns(tableData, queryModel.Columns);
                return tableData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                //BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return null;
        }

        public SchemaData GetDataSourceSchema(SchemaDataInfo schemaData, out string error)
        {
            try
            {
                error = string.Empty;

                var _provider = this.GetDataHelper();

                var result = _provider.GetDataFromWebSource();
                var schema = _provider.GetSchema(result);

                SchemaData rootData = new SchemaData();
                rootData.Name = "Schema";
                rootData.SchemaType = SchemaTypes.Database;

                SchemaData tableData = new SchemaData();
                tableData.Name = _provider.GetTableName();
                tableData.SchemaType = SchemaTypes.Table;

                //tableData.Data = _provider.GetColumnSchema(schema);

                rootData.Data = new List<SchemaData>();
                rootData.Data.Add(tableData);

                return rootData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                //BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
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

                var _provider = this.GetDataHelper();
                var queryModel = JsonHelper.Deserialize<APIQueryModel>(this.Command.Text);

                //if (queryModel.Name.ToLower() != _provider.GetTableName().ToLower())
                //{
                //    throw new Exception("Specify the action or table name is not match");
                //}

                var result = _provider.GetDataFromWebSource();
                _provider.SetExcludeColumns(queryModel.Columns);
                var tableData = _provider.GetData(result);
                _provider.RenameColumns(tableData as System.Data.DataTable, queryModel.Columns);
                return tableData;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                //BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return null;
        }

        public bool TestConnection(out string error)
        {
            error = string.Empty;
            try
            {
                WebDataProvider _provider = this.GetDataHelper();
                _provider.GetDataFromWebSource();
                return _provider.IsConnected;
            }
            catch(Exception ex)
            {
                error = ex.Message;
                //BoldReports.Data.LogHandler.LogError(Helper != null ? Helper.Logger : null, ex.Message, ex, MethodBase.GetCurrentMethod(), ErrorType.Error);
            }
            return false;
        }

        private void ParseDataExpression(ExpressionEventArgs.Expression action)
        {
            if (this.DataSource != null && this.DataSource.ConnectionProperties != null && !string.IsNullOrEmpty(this.DataSource.ConnectionProperties.ConnectString))
            {
                string connectionData = this.DataSource.ConnectionProperties.ConnectString;
                RestAPIModel dataModel = JsonHelper.Deserialize<RestAPIModel>(connectionData);

                dataModel.URL = action.Invoke(dataModel.URL);
                dataModel.RawData = action.Invoke(dataModel.RawData);
                dataModel.UserName = action.Invoke(dataModel.UserName);
                dataModel.Password = action.Invoke(dataModel.Password);

                if (dataModel.Headers != null && dataModel.Headers.Count > 0)
                {
                    foreach (var item in dataModel.Headers)
                    {
                        item.Value = action.Invoke(item.Value);
                    }
                }

                if (dataModel.Parameters != null && dataModel.Parameters.Count > 0)
                {
                    foreach (var item in dataModel.Parameters)
                    {
                        item.Value = action.Invoke(item.Value);
                    }
                }

                this.DataSource.ConnectionProperties.ConnectString = JsonHelper.SerializeObject(dataModel);
            }
        }

        private WebDataProvider GetDataHelper()
        {
            WebDataProvider _provider = new WebDataProvider();

            string connectionData = this.DataSource.ConnectionProperties.ConnectString;
          //  string connectionData = this.ConnectionProperties.ConnectionString;
           // string connectionData = this.ConnectionProperties.ConnectString;
            RestAPIModel dataModel = JsonHelper.Deserialize<RestAPIModel>(connectionData);

            _provider.Url = dataModel.URL;
            _provider.MethodType = (MethodType)Enum.Parse(typeof(MethodType), dataModel.MethodType, true);
            _provider.DataFormat = (DataFormat)Enum.Parse(typeof(DataFormat), dataModel.DataFormat, true);
            _provider.AuthenticationType = (AuthenticationType)Enum.Parse(typeof(AuthenticationType), dataModel.SecurityType, true);
            if (dataModel.BodyType != null)
            {
                _provider.BodyType = (WebRequestBodyType)Enum.Parse(typeof(WebRequestBodyType), dataModel.BodyType, true);
            }
            _provider.RawData = dataModel.RawData;
            _provider.UserName = dataModel.UserName;
            _provider.Password = dataModel.Password;
            this.ConvertValuePairs(_provider.Parameters, dataModel.Parameters);
            this.ConvertValuePairs(_provider.Headers, dataModel.Headers);

            _provider.IsOData = this.IsOdata;

            if (this.IsDesignerMode)
            {
                this.UpdateCustomPropertiesValues(this.ConnectionProperties.CustomProperties, _provider);
            }

            return _provider;
        }

        private void UpdateCustomPropertiesValues(RDL.DOM.CustomProperties customProperties, WebDataProvider _provider)
        {
            if (customProperties != null && customProperties.Count > 0)
            {
                foreach (RDL.DOM.CustomProperty customProperty in customProperties)
                {
                    if (customProperty.Name.Equals("DefaultValues"))
                    {
                        DefaultValues dataModel = JsonHelper.Deserialize<DefaultValues>(customProperty.Value);

                        if (!string.IsNullOrEmpty(dataModel.Url))
                        {
                            _provider.Url = dataModel.Url;
                        }

                        if (!string.IsNullOrEmpty(dataModel.RawData))
                        {
                            _provider.RawData = dataModel.RawData;
                        }

                        if (_provider.Parameters.Count > 0 && dataModel.Parameters.Count > 0)
                        {
                            this.UpdateCollectionValues(_provider.Parameters, dataModel.Parameters);
                        }

                        if (_provider.Headers.Count > 0 && dataModel.Headers.Count > 0)
                        {
                            this.UpdateCollectionValues(_provider.Headers, dataModel.Headers);
                        }

                        if (!string.IsNullOrEmpty(dataModel.UserName))
                        {
                            _provider.UserName = dataModel.UserName;
                        }

                        if (!string.IsNullOrEmpty(dataModel.Password))
                        {
                            _provider.Password = dataModel.Password;
                        }
                    }
                }
            }
        }

        private void UpdateCollectionValues(List<KeyValuePair<string, string>> providerValues, IList<KeyPairModel> modelValues)
        {
            foreach (KeyPairModel modelValue in modelValues)
            {
                for (var index = 0; index < providerValues.Count; index++)
                {
                    if (providerValues[index].Key == modelValue.Key)
                    {
                        providerValues[index] = new KeyValuePair<string, string>(modelValue.Key, modelValue.Value);
                        break;
                    }
                }
            }
        }

        private void ConvertValuePairs(List<KeyValuePair<string, string>> parameters, IList<KeyPairModel> value)
        {
            if(value == null)
            {
                return;
            }

            if(parameters == null)
            {
                parameters = new List<KeyValuePair<string, string>>();
            }

            foreach(var item in value)
            {
                parameters.Add(new KeyValuePair<string, string>(item.Key, item.Value));
            }
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

        public DataTable GetDatabases(out string error)
        {
            throw new NotImplementedException();
        }
    }

    public class RestAPIModel
    {
        public string MethodType { get; set; }
        public string SecurityType { get; set; }
        public string URL { get; set; }
        public string DataFormat { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RawData { get; set; }
        public string BodyType { get; set; }
        public IList<KeyPairModel> Parameters { get; set; }
        public IList<KeyPairModel> Headers { get; set; }
    }

    public class KeyPairModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class APIQueryModel
    {
        public string Name { get; set; }

        public List<APIQueryColumn> Columns { get; set; }
    }

    public class APIQueryColumn
    {
        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public string AliasName { get; set; }

    }

    public class DefaultValues
    {
        public string Url { get; set; }

        public string RawData { get; set; }

        public IList<KeyPairModel> Parameters { get; set; }

        public IList<KeyPairModel> Headers { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
