using BoldReports.Json.Base.Schema;
using BoldReports.Xml.Base.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoldReports.WebDatasource.Base.Model
{
    public class SSLCertificateInfo
    {

        /// <summary>
        /// Client certificate original Name
        /// </summary>
        public string ClientCertificateOriginalName
        {
            get;
            set;
        }

        /// <summary>
        /// Client certificate paswword
        /// </summary>
        public string ClientCertificatePassword
        {
            get;
            set;
        }

        /// <summary>
        /// Byte data of SSL certificate
        /// </summary>
        public byte[] SslCertificateData { get; set; }

        /// <summary>
        /// client Certiticate uploaded filename
        /// </summary>
        public string ClientCertificateFileName
        {
            get; set;
        }
    }


    public enum DataFormat
    {
        Json,
        Csv,
        Xml,
        Excel
    }

    public enum MethodType
    {
        Get,
        Post
    }

    public enum AuthenticationType
    {
        None,
        BasicHttpAuthentication
    }

    public enum WebRequestBodyType
    {
        Parameters = 0,
        Raw = 1
    }

    public class TableSchemaInfo
    {
        public string Id { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public List<TableSchemaInfo> ColumnSchemaInfoCollection { get; set; }
        public string ParentSchemaName { get; set; }
        public bool IsAnonymousSchema { get; set; }
        public JsonCustomSchemaType SchemaType { get; set; }
        public int InnerArrayCount { get; set; }
        public JsonCustomSchemaType FiniteArraySchemaType { get; set; }
        public JsonCustomValueType ValueType { get; set; }
        public bool HasChild { get; set; }
        public string SchemaName { get; set; }
        public string Type { get; set; }
        public XMLCustomSchemaType XmlSchemaType { get; set; }
        public XMLCustomValueType XmlValueType { get; set; }

        public bool IsMapping { get; set; }

        public TableSchemaInfo()
        {
            this.ColumnSchemaInfoCollection = new List<TableSchemaInfo>();
        }
    }
}
