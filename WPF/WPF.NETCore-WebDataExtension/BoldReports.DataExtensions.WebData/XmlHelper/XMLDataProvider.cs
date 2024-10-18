using BoldReports.Xml.Base.Connection;
using BoldReports.Xml.Base.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace BoldReports.Xml.Base
{
    public class XMLDataProvider
    {
        public const string XMLDefaultTableName = "XML Table";

        private const string FileStringSplitter = @"\";

        public const string AnonymousArray = "Anonymous Array";

        private const string UrlStringSplitter = "/";

        #region XML Connection and XML Data Preparation

        /// <summary>
        /// Get the XML connection details based on the Connection type.
        /// </summary>
        /// <param name="isXMLFile">It defines whether the connection belongs to XML file or not</param>
        /// <param name="xmlString">XML data</param>
        /// <param name="connectionPath">It defines the XML connection path</param>
        /// <param name="preferredTableName">Table Name</param>
        /// <returns>It returns the connection details for XML</returns>
        public static XMLConnectionDetails GetXMLConnectionDetails(bool isXMLFile, string xmlString, string connectionPath, string preferredTableName)
        {
            XMLConnectionDetails xmlConnectionDetails = new XMLConnectionDetails()
            {
                PreferredTableName = preferredTableName
            };

            if (isXMLFile)
            {
                xmlConnectionDetails.SourceConnectionType = XMLConnectionType.File;
                xmlConnectionDetails.SourceFileConnectionInfo = new XMLFileConnectionDetails()
                {
                    Path = connectionPath
                };
            }
            else
            {
                xmlConnectionDetails.SourceConnectionType = XMLConnectionType.WebDataConnector;
                xmlConnectionDetails.SourceWebConnectionInfo = new XMLWebConnectionDetails
                {
                    Url = connectionPath,
                    XMLString = xmlString
                };
            }

            return xmlConnectionDetails;
        }

        /// <summary>
        /// It returns the XML data based on the data provider type.
        /// </summary>
        /// <param name="xmlConnectionDetails">XML connection details</param>
        /// <returns>Returns the key value pair for the XML data</returns>
        private static KeyValuePair<string, string> GetXMLString(XMLConnectionDetails xmlConnectionDetails)
        {
            bool hasTableName = !string.IsNullOrWhiteSpace(xmlConnectionDetails.PreferredTableName);
            string tableName = hasTableName ? xmlConnectionDetails.PreferredTableName : XMLDefaultTableName;
            string xmlString = string.Empty;
            switch (xmlConnectionDetails.SourceConnectionType)
            {
                case XMLConnectionType.File:
                    string filePath = xmlConnectionDetails.SourceFileConnectionInfo.Path;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            xmlString = reader.ReadToEnd();
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

                case XMLConnectionType.WebDataConnector:
                    xmlString = xmlConnectionDetails.SourceWebConnectionInfo.XMLString;
                    if (!hasTableName)
                    {
                        string webUrl = xmlConnectionDetails.SourceWebConnectionInfo.Url;
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

            return new KeyValuePair<string, string>(tableName, xmlString);
        }

        /// <summary>
        /// Removes the unwanted namespace data from the XML data
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        internal static string RemoveAllNamespaces(string xmlString)
        {
           return RemoveAllNamespaces(XElement.Parse(xmlString)).ToString();
        }

        /// <summary>
        /// Removes the unwanted namespace data from the XML data
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;
                foreach (XAttribute attribute in xmlDocument.Attributes())
                {
                    xElement.Add(attribute);
                }

                return xElement;
            }

            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        #endregion

        #region Get XML Schema Info

        /// <summary>
        /// Get the XML connection info and parse the XML and return the XML schema for the data.
        /// </summary>
        /// <param name="xmlConnectionDetails">COllection of XML connection</param>
        /// <returns></returns>
        public static XMLTableInfo GetXMLSourceTableWithAllSchemaInfos(XMLConnectionDetails xmlConnectionDetails)
        {
            XMLTableInfo resultTableInfo = null;
            if (xmlConnectionDetails != null)
            {
                KeyValuePair<string, string> xmlData = GetXMLString(xmlConnectionDetails);
                if (xmlData.Value != null)
                {
                    resultTableInfo = new XMLTableInfo
                    {
                        TableName = xmlData.Key,
                        SelectedXMLSchemas = GetAllXMLSchemaInfos(xmlData.Value)
                    };
                }
            }

            return resultTableInfo;
        }

        /// <summary>
        /// Get the XML schema set for the XML data.
        /// </summary>
        /// <param name="xmlObject"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static XmlSchemaSet GetXmlSchemaSet(string xmlObject, out string data)
        {
            data = xmlObject;
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XPathDocument xmlDoc = new XPathDocument(new StringReader(xmlObject));
            XPathNavigator pathNavigator = xmlDoc.CreateNavigator();
            pathNavigator.MoveToFollowing(XPathNodeType.Element);
            IDictionary<string, string> namespaceCollection = pathNavigator.GetNamespacesInScope(XmlNamespaceScope.All);

            if (namespaceCollection.Count > 1)
            {
                data = RemoveAllNamespaces(xmlObject);
            }

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    schemaSet = new XmlSchemaInference().InferSchema(reader);
                }
            }

            return schemaSet;
        }

        /// <summary>
        /// Returns the collection of XML schema info for the XML data.
        /// </summary>
        /// <param name="xmlObject"></param>
        /// <returns></returns>
        private static ObservableCollection<XMLSchemaInfo> GetAllXMLSchemaInfos(string xmlObject)
        {
            string data = string.Empty;
            ObservableCollection<XMLSchemaInfo> xmlSchemaInfoCollection = new ObservableCollection<XMLSchemaInfo>();
            var schemas = GetXmlSchemaSet(xmlObject, out data)?.Schemas();

            if (schemas == null)
                return new ObservableCollection<XMLSchemaInfo>();

            foreach (XmlSchema schema in schemas)
            {
                foreach (XmlSchemaElement schemaElements in schema.Elements.Values)
                {
                    xmlSchemaInfoCollection.Add(XMLSchemaInfoUtilities.GetAllXmlSchemaInfo(schemaElements));
                }
            }

            return xmlSchemaInfoCollection;
        }

        #endregion

        #region Get XML Tables

        /// <summary>
        /// Get the table for the XML connection details based on table schema info
        /// </summary>
        /// <param name="xmlConnectionDetails">Details of the XML connection info</param>
        /// <param name="xmlTableInfoCollection">Collection of XML table info</param>
        /// <returns></returns>
        public static ObservableCollection<DataTable> GetXMLTables(XMLConnectionDetails xmlConnectionDetails, ObservableCollection<XMLTableInfo> xmlTableInfoCollection)
        {
            ObservableCollection<DataTable> tables = new ObservableCollection<DataTable>();
            if (xmlConnectionDetails != null && xmlTableInfoCollection != null)
            {
                string data = string.Empty;
                GetXmlSchemaSet(GetXMLString(xmlConnectionDetails).Value, out data);
                if (!string.IsNullOrEmpty(data))
                {
                    foreach (XMLTableInfo xmlTableInfo in xmlTableInfoCollection)
                    {
                        DataTable table = GetXMLTable(xmlTableInfo.SelectedXMLSchemas, data);

                        if (table != null)
                        {
                            table.TableName = xmlTableInfo.TableName;
                            XMLTabularUtilities.TrimDataColumnNames(table, xmlConnectionDetails.SchemaDetails);
                            tables.Add(table);
                        }
                    }
                }
            }

            return tables;
        }

        /// <summary>
        /// Get the table for the XML connection details based on table schema info
        /// </summary>
        /// <param name="selectedXmlSchemaInfos">Details of the XML schema based on the XML data</param>
        /// <param name="xmlData">XML data</param>
        /// <returns></returns>
        private static DataTable GetXMLTable(ObservableCollection<XMLSchemaInfo> selectedXmlSchemaInfos, string xmlData)
        {
            ObservableCollection<XMLSchemaInfo> tempSelectedXMLSchemaInfos = selectedXmlSchemaInfos != null
                && selectedXmlSchemaInfos.Count > 0 ? selectedXmlSchemaInfos : GetAllXMLSchemaInfos(xmlData);

            if (!string.IsNullOrEmpty(xmlData) && tempSelectedXMLSchemaInfos != null && tempSelectedXMLSchemaInfos.Count > 0)
            {
                return XMLTabularUtilities.GetXMLTable(tempSelectedXMLSchemaInfos, xmlData);
            }

            return new DataTable();
        }

        #endregion

    }
}
