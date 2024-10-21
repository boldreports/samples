
using BoldReports.Xml.Base.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace BoldReports.Xml.Base
{
    public class XMLTabularUtilities
    {

        /// <summary>
        /// Get the XML Table Value based on selected table schema Info
        /// </summary>
        /// <param name="selectedXmlSchemaInfos">It defines the selected table schema Info</param>
        /// <param name="xmlString">It defines the XML string</param>
        /// <returns>Returns the Data table for the XML data</returns>
        internal static DataTable GetXMLTable(ObservableCollection<XMLSchemaInfo> selectedXmlSchemaInfos, string xmlString)
        {
            return ProcessXMLData(null, selectedXmlSchemaInfos, xmlString);
        }

        /// <summary>
        /// Get the XML Table Value based on selected table schema Info
        /// </summary>
        /// <param name="parentSchemaNames">It defines the parent schema of the current selected table schema</param>
        /// <param name="selectedXmlSchemaInfos">It defines the selected table schema Info</param>
        /// <param name="xmlString">It defines the XML string</param>
        /// <returns>Returns the Data table for the XML data</returns>
        private static DataTable ProcessXMLData(ICollection<string> parentSchemaNames, IEnumerable<XMLSchemaInfo> selectedXmlSchemaInfos, string xmlString)
        {
            DataTable table = null;
            foreach (XMLSchemaInfo xmlSchemaInfo in selectedXmlSchemaInfos)
            {
                List<string> tempSchemaNames = GetSchemaNames(parentSchemaNames, xmlSchemaInfo);
                DataTable schemaItemTable = null;

                switch (xmlSchemaInfo.SchemaType)
                {
                    case XMLCustomSchemaType.XmlSchemaSimpleType:
                        {
                            List<string> values = GetXmlValueByTagName(GetTagName(tempSchemaNames), xmlString);

                            string columnName = GetColumnName(tempSchemaNames);
                            schemaItemTable = new DataTable
                            {
                                Locale = CultureInfo.InvariantCulture
                            };

                            schemaItemTable.Columns.Add(columnName, GetDataType(xmlSchemaInfo.ValueType));
                            foreach (string value in values)
                            {
                                DataRow row = schemaItemTable.NewRow();
                                schemaItemTable.Rows.Add(row);

                                if (value != null)
                                {
                                    row[columnName] = value;
                                }
                            }
                        }
                        break;

                    case XMLCustomSchemaType.XmlSchemaComplexType:
                        {
                            if (xmlSchemaInfo.ChildSchemas.Count > 0)
                            {
                                List<string> elements = GetXmlElementByTagName(GetTagName(tempSchemaNames), xmlString);
                                schemaItemTable = new DataTable
                                {
                                    Locale = CultureInfo.InvariantCulture
                                };
                                foreach (string element in elements)
                                {
                                    DataTable tab = ProcessXMLData(tempSchemaNames, xmlSchemaInfo.ChildSchemas, element);
                                    schemaItemTable = Merge(schemaItemTable, tab, MergeType.Row);
                                }
                            }
                            else
                            {
                                List<string> complexValues = GetXmlValueByTagName(GetTagName(tempSchemaNames), xmlString);
                                string columnName = GetColumnName(tempSchemaNames);
                                schemaItemTable = new DataTable
                                {
                                    Locale = CultureInfo.InvariantCulture
                                };
                                schemaItemTable.Columns.Add(columnName, GetDataType(xmlSchemaInfo.ValueType));
                                foreach (string value in complexValues)
                                {
                                    DataRow row = schemaItemTable.NewRow();
                                    schemaItemTable.Rows.Add(row);

                                    if (value != null)
                                    {
                                        row[columnName] = value;
                                    }
                                }
                            }
                        }

                        break;

                    case XMLCustomSchemaType.XmlSchemaArrayType:
                        {
                            List<string> arrayValues = GetXmlElementByTagName(GetTagName(tempSchemaNames), xmlString);
                            schemaItemTable = new DataTable
                            {
                                Locale = CultureInfo.InvariantCulture
                            };
                            foreach (string element in arrayValues)
                            {
                                DataTable tab = ProcessXMLData(tempSchemaNames, xmlSchemaInfo.ChildSchemas, element);

                                List<string> val = GetXmlValueByTagName(GetTagName(tempSchemaNames), element);
                                schemaItemTable = Merge(schemaItemTable, tab, MergeType.Row);
                            }
                        }

                        break;
                }

                table = Merge(table, schemaItemTable, MergeType.Matrix);
            }

            return table;
        }

        /// <summary>
        /// Get the temporary schema names as collection.
        /// </summary>
        /// <param name="parentSchemaNames">It defines the patent schema collection of current schema.</param>
        /// <param name="xmlSchemaInfo">XMml schema info details of the current Schema.</param>
        /// <returns>Returns the collection of parent schema for selected schema.</returns>
        private static List<string> GetSchemaNames(ICollection<string> parentSchemaNames, XMLSchemaInfo xmlSchemaInfo)
        {
            List<string> tempSchemaNames = new List<string>();
            if (parentSchemaNames != null)
            {
                tempSchemaNames.AddRange(parentSchemaNames);
            }

            if (!xmlSchemaInfo.IsAnonymousSchema)
            {
                tempSchemaNames.Add(xmlSchemaInfo.SchemaName);
            }

            return tempSchemaNames;
        }

        /// <summary>
        /// Get the element or tag name.
        /// </summary>
        /// <param name="tempSchemaNames">It defines the collection of parent schema for selected schema.</param>
        /// <returns>Returns the element or tag name.</returns>
        static string GetTagName(List<string> tempSchemaNames)
        {
            string tagName = string.Empty;

            foreach(string tag in tempSchemaNames)
            {
                tagName = tagName + "/" + tag;
            }

            return tagName;
        }

        /// <summary>
        /// Get the XML elements based on the XML tags.
        /// </summary>
        /// <param name="tag">it defines the XML tag</param>
        /// <param name="xmlString">It defines the XML string.</param>
        /// <returns></returns>
        private static List<string> GetXmlElementByTagName(string tag, string xmlString)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);

            XmlNodeList data = xmlDocument.SelectNodes(tag);
            List<string> value = new List<string>();
            if (data.Count > 0)
            {
                int count = data.Count;
                for (int i = 0; i < count; i++)
                {
                    value.Add(data.Item(i).OuterXml);
                }
            }
            else
            {
                string ele = tag.Substring(tag.LastIndexOf("/")).Trim('/');
                data = xmlDocument.DocumentElement.ChildNodes;
                for (int i = 0; i < data.Count; i++)
                {
                    string currentNodeName = data.Item(i).Name;
                    string tempName = tag.Substring(tag.LastIndexOf('/'), tag.Length- tag.LastIndexOf('/')).Trim('/');
                    if (currentNodeName.Equals(tempName))
                    {
                        value.Add(data.Item(i).OuterXml);
                    }
                }
            }

            return value;
        }

        private static List<string> GetXmlValueByTagName(string tag, string xmlString)
        {
            XmlDocument xmlDocument = new XmlDocument();
            List<string> value = new List<string>();

            xmlDocument.LoadXml(xmlString);
            try
            {
                // It works for when the XML child element name also in the same of the current element.
                XmlNodeList xmlNode = xmlDocument.SelectNodes(tag);
                if (xmlNode.Count > 0)
                {
                    int count = xmlNode.Count;
                    for (int i = 0; i < count; i++)
                    {
                        value.Add(xmlNode.Item(i).InnerXml);
                    }
                }
                else
                {
                    string ele = tag.Substring(tag.LastIndexOf('/')).Trim('/');
                    xmlNode = xmlDocument.DocumentElement.ChildNodes;
                    if (xmlNode.Count > 0) // used to get the values of the sub elements(particles) the root elements.
                    {
                        for (int i = 0; i < xmlNode.Count; i++)
                        {
                            string currentNodeName = xmlNode.Item(i).Name;
                            string tempName = tag.Substring(tag.LastIndexOf('/'), tag.Length - tag.LastIndexOf('/')).Trim('/');
                            if (currentNodeName.Equals(tempName))
                            {
                                if (xmlNode.Item(i).ChildNodes.Count == 1)
                                {
                                    value.Add(xmlNode.Item(i).InnerText);
                                }
                            }
                        }
                    }
                    if(value.Count.Equals(0)) // used to get the element values(called as attributes and attribute uses) available in the root element.
                    {
                        string attrValue = xmlDocument.DocumentElement.Attributes[ele]?.Value;
                        if (attrValue == null)
                        {
                            XmlNodeList data = xmlDocument.GetElementsByTagName(ele);
                            if (data.Count > 0)
                            {
                                attrValue = data.Item(0).InnerXml;
                            }
                        }
                        if (!string.IsNullOrEmpty(attrValue))
                        {
                            value.Add(attrValue);
                        }
                    }
                }
            }
            catch
            {
            }

            return value;
        }

        /// <summary>
        /// Get the data type of the XML value.
        /// </summary>
        /// <param name="xmlValueType">XMLCustomValueType</param>
        /// <returns></returns>
        internal static Type GetDataType(XMLCustomValueType xmlValueType)
        {
            Type type = typeof(object);
            switch (xmlValueType)
            {
                case XMLCustomValueType.String:
                case XMLCustomValueType.normalizedString:
                    type = typeof(string);
                    break;
                case XMLCustomValueType.Double:
                case XMLCustomValueType.integer:
                case XMLCustomValueType.Int:
                case XMLCustomValueType.negativeInteger:
                case XMLCustomValueType.nonNegativeInteger:
                case XMLCustomValueType.positiveInteger:
                case XMLCustomValueType.nonPositiveInteger:
                case XMLCustomValueType.Float:
                case XMLCustomValueType.Long:
                case XMLCustomValueType.unsignedLong:
                case XMLCustomValueType.unsignedInt:
                case XMLCustomValueType.unsignedShort:
                    type = typeof(double);
                    break;
                case XMLCustomValueType.Decimal:
                    type = typeof(decimal);
                    break;
                case XMLCustomValueType.boolean:
                    type = typeof(bool);
                    break;
                case XMLCustomValueType.dateTime:
                case XMLCustomValueType.date:
                case XMLCustomValueType.gMonthDay:
                case XMLCustomValueType.gDay:
                case XMLCustomValueType.gYear:
                case XMLCustomValueType.gYearMonth:
                case XMLCustomValueType.month:
                case XMLCustomValueType.time:
                case XMLCustomValueType.timePeriod:
                case XMLCustomValueType.duration:
                    type = typeof(DateTime);
                    break;
                case XMLCustomValueType.Byte:
                case XMLCustomValueType.unsignedByte:
                    type = typeof(byte);
                    break;
                case XMLCustomValueType.anyUri:
                    type = typeof(Uri);
                    break;
            }

            return type;
        }

        /// <summary>
        /// Trim the table column name based on the other column names.
        /// </summary>
        /// <param name="table">DataTable</param>
        internal static void TrimDataColumnNames(DataTable table, List<string> columns)
        {
            if (table != null)
            {
                List<CustomColumnNameInfo> customNames = new List<CustomColumnNameInfo>();
                foreach (DataColumn dataColumn in table.Columns)
                {
                    customNames.Add(new CustomColumnNameInfo(dataColumn.ColumnName));
                }

                foreach (CustomColumnNameInfo customName in customNames)
                {
                    CustomColumnNameInfo[] tempDuplicateNames = customNames.Where(tempName => tempName.ColumnName == customName.ColumnName).ToArray();
                    if (tempDuplicateNames.Length <= 1)
                    {
                        if (!(columns != null && columns.Count > 0 && tempDuplicateNames.Length == 1 && columns.Where(t => t != null && t.ToLower() == customName.ColumnName.ToLower()).Count() > 1))
                            continue;
                    }

                    foreach (CustomColumnNameInfo customColumnNameInfo in tempDuplicateNames)
                    {
                        customColumnNameInfo.PrepareSimplifiedUniqueColumnName();
                    }
                }

                foreach (CustomColumnNameInfo customName in customNames)
                {
                    DataColumn column = table.Columns[customName.ColumnFullName];
                    column.ColumnName = customName.ColumnName;
                }
            }
        }

        /// <summary>
        /// Get the column names based on the schema name from table.
        /// </summary>
        /// <param name="schemaNames"></param>
        /// <returns></returns>
        private static string GetColumnName(List<string> schemaNames)
        {
            string columnName = string.Empty;
            if (schemaNames != null && schemaNames.Count > 0)
            {
                schemaNames.ForEach(key => columnName = columnName.Length == 0 ?
                    key : columnName + XMLProperties.GetUniqueSplitter() + key);
            }
            return columnName;
        }

        /// <summary>
        /// Merge the data table based on merge type.
        /// </summary>
        /// <param name="firstTable"></param>
        /// <param name="secondTable"></param>
        /// <param name="mergeType">Type of the Merge(Row, Matrix)</param>
        /// <returns></returns>
        private static DataTable Merge(DataTable firstTable, DataTable secondTable, MergeType mergeType)
        {
            DataTable table = null;
            if (firstTable != null && firstTable.Rows.Count > 0 &&
                secondTable != null && secondTable.Rows.Count > 0)
            {
                switch (mergeType)
                {
                    case MergeType.Matrix:
                        table = MatrixMerge(firstTable, secondTable);
                        break;
                    case MergeType.Row:
                        table = RowMerge(firstTable, secondTable);
                        break;
                }
            }
            else if (firstTable != null && firstTable.Rows.Count > 0)
            {
                table = firstTable;
                if (mergeType == MergeType.Row)
                {
                    table.Rows.Add(table.NewRow());
                }
            }
            else if (secondTable != null && secondTable.Rows.Count > 0)
            {
                table = secondTable;
            }
            else
            {
                if (mergeType == MergeType.Row)
                {
                    table = new DataTable
                    {
                        Locale = CultureInfo.InvariantCulture
                    };
                    table.Rows.Add(table.NewRow());
                }
            }

            return table;
        }

        /// <summary>
        /// Merge the two different data tables in a matrix type.
        /// </summary>
        /// <param name="firstTable"></param>
        /// <param name="secondTable"></param>
        /// <returns></returns>
        private static DataTable MatrixMerge(DataTable firstTable, DataTable secondTable)
        {
            DataTable table = new DataTable
            {
                Locale = CultureInfo.InvariantCulture
            };

            int totalColumns = firstTable.Columns.Count + secondTable.Columns.Count;
            foreach (DataColumn dataColumn in firstTable.Columns)
            {
                table.Columns.Add(new DataColumn(dataColumn.ColumnName, dataColumn.DataType));
            }

            foreach (DataColumn dataColumn in secondTable.Columns)
            {
                table.Columns.Add(new DataColumn(dataColumn.ColumnName, dataColumn.DataType));
            }

            foreach (DataRow firstTableRow in firstTable.Rows)
            {
                foreach (DataRow secondTableRow in secondTable.Rows)
                {
                    DataRow row = table.NewRow();

                    for (int i = 0; i < totalColumns; i++)
                    {
                        if (i < firstTable.Columns.Count)
                        {
                            row[i] = firstTableRow[i];
                        }
                        else
                        {
                            int index = i - firstTable.Columns.Count;
                            row[i] = secondTableRow[index];
                        }
                    }

                    table.Rows.Add(row);
                }
            }

            ClearTableData(firstTable);
            ClearTableData(secondTable);
            return table;
        }

        /// <summary>
        /// Merge the two different tables in a row format.
        /// </summary>
        /// <param name="firstTable"></param>
        /// <param name="secondTable"></param>
        /// <returns></returns>
        private static DataTable RowMerge(DataTable firstTable, DataTable secondTable)
        {
            firstTable.Merge(secondTable, true, MissingSchemaAction.Add);
            ClearTableData(secondTable);
            return firstTable;
        }

        /// <summary>
        /// Clear the table data.
        /// </summary>
        /// <param name="table"></param>
        private static void ClearTableData(DataTable table)
        {
            if (table != null)
            {
                table.Rows.Clear();
                table.Columns.Clear();
                table.Dispose();
            }
        }
    }

    /// <summary>
    /// Merge types for the tables.
    /// </summary>
    internal enum MergeType
    {
        Matrix,

        Row
    }
}
