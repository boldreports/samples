//-------------------------------------------------------------------------------------------------
// <copyright file="SqlDataProvider.cs" company="syncfusion">
//  Copyright (c) Syncfusion Inc. 2001 - 2010. All rights reserved.
//  Use of this code is subject to the terms of our license.
//  A copy of the current license can be obtained at any time by e-mailing
//  licensing@syncfusion.com. Re-distribution in any form is strictly
//  prohibited. Any infringement will be prosecuted under applicable laws.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace BoldReports.Data.WebData
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml.Linq;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Globalization;
    using BoldReports.RDL.DOM;
    using BoldReports.Windows.Data;

    class TableColumn
    {
        public string ColumnName { get; set; }
        public int TableLevel { get; set; }
    }

    internal class XmlDataExtension : DataExtensionBase
    {
        private Dictionary<object, object> XmlRefValue = null;

        public XmlDataExtension()
        {
            this.CustomProperties = new Dictionary<string, object>();
            this.CustomProperties.Add("QueryDesignerEnabled", "false");
            this.CustomProperties.Add("QueryFilterEnabled", "false");
            this.CustomProperties.Add("QueryExpressionEnabled", "false");
            this.CustomProperties.Add("QueryJoinerEnabled", "false");
            this.CustomProperties.Add("QueryColumnEdit", "false");
            this.CustomProperties.Add("QueryParameterEnabled", "false");
        }

        public override object GetData(out string error)
        {
            try
            {
                error = string.Empty;
                var list = new List<Dictionary<string, object>>();
                var table = GetTable(this.ConnectionProperties.ConnectionString, this.Command.Text, "Table");

                if (table != null)
                {
                    Dictionary<string, System.Type> fieldTypes = this.DataSet != null && this.DataSet.Fields != null ? this.DataSet.Fields.Where(field => field.DataField != null && field.TypeName != null)
                        .ToDictionary(field => field.DataField, field => System.Type.GetType(field.TypeName)) : null;

                    foreach (System.Data.DataRow row in table.Rows)
                    {
                        var data = new Dictionary<string, object>();

                        foreach (DataColumn columninfo in table.Columns)
                        {
                            object value = null;

                            try
                            {
                                value = (fieldTypes != null && fieldTypes.Keys.Contains(columninfo.ColumnName) && row[columninfo.ColumnName] != System.DBNull.Value) ?
                                Convert.ChangeType(row[columninfo.ColumnName], fieldTypes[columninfo.ColumnName], CultureInfo.CurrentCulture) : row[columninfo.ColumnName];
                            }
                            catch
                            {
                                value = row[columninfo.ColumnName];
                            }

                            if (value == System.DBNull.Value)
                            {
                                value = null;
                            }

                            data.Add(columninfo.ColumnName, value);
                        }

                        list.Add(data);
                    }
                }

                return list;
            }
            catch (Exception e)
            {
                error = e.Message;
                throw;
            }
        }

        public override bool TestConnection(out string error)
        {
            try
            {
                var connectionStr = this.ConnectionProperties.ConnectionString;

                error = string.Empty;

                if (string.IsNullOrEmpty(connectionStr))
                {
                    return true;
                }
                else if ((connectionStr.StartsWith("http:/", StringComparison.InvariantCultureIgnoreCase) || connectionStr.StartsWith("https:/", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(connectionStr))
                {
                    return IsValidXML(connectionStr);
                }
                else if (!(connectionStr.StartsWith("http:/", StringComparison.InvariantCultureIgnoreCase) || connectionStr.StartsWith("https:/", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(connectionStr))
                {
                    return IsValidXML(connectionStr);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (!string.IsNullOrEmpty(this.ConnectionProperties.ConnectionString))
            {
                return true;
            }

            return false;
        }

        public bool ValidateQuery(string elementPath)
        {
            string pathPattern = @"^(\s*[0-9a-zA-Z]*\s*({(\s*(,\s*)?[\@]\s*[0-9a-zA-Z]*\s*)*}\s*)?(/\s*[0-9a-zA-Z]+\s*({(\s*(,)?[\@][0-9a-zA-Z]*\s*)*}\s*)?)*)$";

            if (System.Text.RegularExpressions.Regex.IsMatch(elementPath, pathPattern))
            {
                string temp = elementPath.Replace('{', '$');
                temp = temp.Replace('}', '$');
                string[] references = temp.Split('$').ToArray();
                references = (from str in references where str.Contains("@") select str).ToArray();
                int count = 0;
                foreach (var str in references)
                {
                    count += str.Split(',').Select(s => s.Trim()).Where(s => s.Length == 1).Count();
                    if (count > 1)
                        throw new DataException("The XmlDP query contains more than one reference to the attribute/element @.");
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void ProcessElementPath(XElement xmldata, string elementPath)
        {
            string[] subPaths = elementPath.Split('/');
            string modifiedPath = elementPath.Replace(subPaths[0] + "/", "");
            IEnumerable<XElement> elementList = xmldata.Elements();

            if (xmldata != null && subPaths != null)
            {
                foreach (var element in elementList.ToList())
                {
                    if (element.HasElements)
                    {
                        ProcessElementPath(element, modifiedPath);
                    }
                }
            }

            subPaths = subPaths.Select(str => str.Trim()).ToArray();
            int index = subPaths[0].IndexOf('{');
            int length = subPaths[0].IndexOf('}') - (index + 1);
            string references = null;

            if (length > -1)
            {
                references = subPaths[0].Substring(index + 1, length);
                modifiedPath = subPaths[0].Substring(0, index);

                for (int i = 0; i < subPaths.Count(); i++)
                {
                    if (subPaths[i].Equals(subPaths[0]))
                    {
                        subPaths[i] = modifiedPath;
                    }
                }
            }

            string[] attributes = references != null ? references.Split(',') : null;

            if (attributes != null && subPaths.Contains(xmldata.Name.LocalName))
            {
                XElement currentElement = XElement.Parse(xmldata.ToString());
                string current = xmldata.Name.LocalName;
                bool removeChild = (from node in xmldata.Elements()
                                    where subPaths.Contains(node.Name.LocalName)
                                    select node).Count() == 0;

                if (xmldata.HasElements && removeChild)
                {
                    xmldata.Nodes().Remove();
                }
                if (attributes.Count() == 1 && string.IsNullOrEmpty(attributes[0]))
                {
                    if (removeChild)
                        xmldata.Remove();
                    else
                        xmldata.RemoveAttributes();
                }
                else
                {
                    if (attributes.Contains("@"))
                    {
                        StringBuilder value = new StringBuilder();

                        if (XmlRefValue == null)
                        {
                            XmlRefValue = new Dictionary<object, object>();
                        }
                        foreach (var node in currentElement.Elements())
                        {
                            value.Append(node.ToString());
                        }

                        string objectID = Guid.NewGuid().ToString().Split('-')[0];
                        XmlRefValue.Add(objectID, value);
                        xmldata.SetValue(objectID);
                        attributes = (from str in attributes where !str.Equals("@") select str).ToArray();
                    }
                    if (attributes.Count() != 0)
                    {
                        attributes = (from str in attributes select str.Replace("@", "")).ToArray();

                        foreach (var attrib in attributes)
                        {
                            if (xmldata.Attribute(attrib) == null)
                            {
                                xmldata.Add(new XAttribute(attrib, ""));
                            }
                            else
                            {
                                xmldata.Attribute(attrib).Remove();
                            }
                        }
                        var ignoreattrib = from attrib in xmldata.Attributes()
                                           where !attributes.Contains(attrib.Name.LocalName)
                                           select attrib.Name;
                        foreach (var attrib in ignoreattrib.ToList())
                        {
                            xmldata.Attribute(attrib).Remove();
                        }
                    }
                }
            }
            else if (!subPaths.Contains(xmldata.Name.LocalName))
            {
                xmldata.Remove();
            }
        }

        private bool IsValidXML(string xmlFile)
        {
            try
            {
                XDocument xd1 = new XDocument();
                xd1 = XDocument.Load(xmlFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetTableData()
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            string elementPath = string.Empty;
            XElement data = null;

            if (!string.IsNullOrEmpty(this.ConnectionProperties.ConnectionString))
            {
                string xmlPath = string.Empty;

                if ((this.ConnectionProperties.ConnectionString.StartsWith("http:/", StringComparison.InvariantCultureIgnoreCase) || this.ConnectionProperties.ConnectionString.StartsWith("https:/", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(this.ConnectionProperties.ConnectionString))
                {
                    xmlPath = this.ConnectionProperties.ConnectionString;
                }
                else if (!(this.ConnectionProperties.ConnectionString.StartsWith("http:/", StringComparison.InvariantCultureIgnoreCase) || this.ConnectionProperties.ConnectionString.StartsWith("https:/", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(this.ConnectionProperties.ConnectionString))
                {
                    xmlPath = this.ConnectionProperties.ConnectionString;
                }

                XmlReader reader = System.Xml.XmlReader.Create(xmlPath);
                data = XElement.Load(reader);

                if (!string.IsNullOrEmpty(this.Command.Text))
                {
                    elementPath = this.Command.Text;

                    if (this.Command.Text.Contains("ElementPath"))
                    {
                        XmlReader queryReader = XmlReader.Create(new System.IO.StringReader(this.Command.Text));
                        XElement xQuery = XElement.Load(queryReader);
                        XElement x_elementPath = xQuery.Elements("ElementPath").FirstOrDefault();
                        elementPath = x_elementPath.Value;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.Command.Text))
            {
                System.Xml.XmlReader stream = System.Xml.XmlReader.Create(new System.IO.StringReader(this.Command.Text));
                XElement xQuery = XElement.Load(stream);
                data = xQuery.Elements("XmlData").Elements().FirstOrDefault();
                IEnumerable<XElement> x_elementPath = xQuery.Elements("ElementPath");

                if (x_elementPath != null && x_elementPath.Count() > 0)
                {
                    elementPath = x_elementPath.FirstOrDefault().Value;
                }
            }

            if (data != null)
            {
                if (!string.IsNullOrEmpty(elementPath))
                {
                    if (ValidateQuery(elementPath))
                    {
                        ProcessElementPath(data, elementPath);
                    }
                }

                StringReader sr = new StringReader(data.ToString());
                ds.ReadXml(sr);
                DataTable dataTable = this.CreateJoinTable(ds);
                dataTable.TableName = "Table";
                return dataTable;
            }

            throw new DataException("Datasource does not support");
        }

        public DataTable GetTable(string connectionString, string command, string tableName)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            string elementPath = string.Empty;
            XElement data = null;

            if (!string.IsNullOrEmpty(connectionString))
            {
                string xmlPath = connectionString;

                if (this.Helper != null)
                {
                    string reportPath = this.Helper.ReportPath;

                    if (!System.IO.Path.IsPathRooted(xmlPath) && !string.IsNullOrEmpty(reportPath) && !(xmlPath.Contains("http")))
                    {
                        xmlPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(reportPath), xmlPath);
                    }
                }

                XmlReader reader = System.Xml.XmlReader.Create(xmlPath);
                data = XElement.Load(reader);

                if (!string.IsNullOrEmpty(command))
                {
                    elementPath = command;

                    if (command.Contains("ElementPath"))
                    {
                        XmlReader queryReader = XmlReader.Create(new System.IO.StringReader(command));
                        XElement xQuery = XElement.Load(queryReader);
                        XElement x_elementPath = xQuery.Elements("ElementPath").FirstOrDefault();
                        elementPath = x_elementPath.Value;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(command))
            {
                System.Xml.XmlReader stream = System.Xml.XmlReader.Create(new System.IO.StringReader(command));
                XElement xQuery = XElement.Load(stream);
                data = xQuery.Elements("XmlData").Elements().FirstOrDefault();
                IEnumerable<XElement> x_elementPath = xQuery.Elements("ElementPath");

                if (x_elementPath != null && x_elementPath.Count() > 0)
                {
                    elementPath = x_elementPath.FirstOrDefault().Value;
                }
            }

            if (data != null)
            {
                if (!string.IsNullOrEmpty(elementPath))
                {
                    if (ValidateQuery(elementPath))
                    {
                        ProcessElementPath(data, elementPath);
                    }
                }
                StringReader sr = new StringReader(data.ToString());
                ds.ReadXml(sr);
                DataTable dataTable = this.CreateJoinTable(ds);
                dataTable.TableName = tableName;
                return dataTable;
            }

            throw new Exception("Datasource does not support");
        }

        DataTable CreateJoinTable(System.Data.DataSet set)
        {
            if (set.Tables.Count == 1 && this.XmlRefValue == null)
            {
                return set.Tables[0];
            }

            DataTable dataSourceTable = new DataTable();
            if (set.Tables.Count > 0)
            {
                DataTable partable = set.Tables[set.Tables.Count - 1];
                List<TableColumn> columnCollection = new List<TableColumn>();
                List<string> relations = new List<string>();
                int level = -1;

                for (int i = set.Tables.Count - 1; i >= 0; i--)
                {
                    DataTable sourceTable = set.Tables[i];
                    level++;

                    foreach (DataColumn column in sourceTable.Columns)
                    {
                        bool columnAdded = true;

                        if (sourceTable.ParentRelations != null && sourceTable.ParentRelations.Count > 0)
                        {
                            var collumnDetails = from col in sourceTable.ParentRelations[0].ChildColumns where col.ColumnName == column.ColumnName select col;
                            columnAdded = collumnDetails.Count() == 0;

                            if (collumnDetails.Count() > 0)
                            {
                                relations.Add(sourceTable.ParentRelations[0].RelationName);
                            }
                        }

                        if (columnAdded && !dataSourceTable.Columns.Contains(column.ColumnName))
                        {
                            columnCollection.Add(new TableColumn() { ColumnName = column.ColumnName, TableLevel = level });
                            if (column.ColumnName.EndsWith("_Text"))
                            {
                                dataSourceTable.Columns.Add(column.ColumnName.Replace("_Text", ""), column.DataType, column.Expression);
                            }
                            else
                            {
                                dataSourceTable.Columns.Add(column.ColumnName, column.DataType, column.Expression);
                            }
                        }
                    }
                }

                if (set.Tables.Count > 1)
                {
                    try
                    {
                        int max = partable.Rows.Count;
                        DataTable table = null;
                        foreach (DataTable tab in set.Tables)
                        {
                            if (tab != null && tab.Rows != null && tab.Rows.Count > max)
                            {
                                table = tab;
                                max = tab.Rows.Count;
                            }
                        }
                        if (table != null)
                        {
                            partable = table;
                        }
                    }
                    catch
                    {
                        partable = set.Tables[set.Tables.Count - 1];
                    }
                }

                foreach (System.Data.DataRow row in partable.Rows)
                {
                    List<object> datas = new List<object>();

                    foreach (var column in columnCollection)
                    {
                        System.Data.DataRow dataRow = row;

                        try
                        {
                            for (int i = 0; i < column.TableLevel; i++)
                            {
                                if (dataRow != null)
                                {
                                    dataRow = dataRow.GetParentRow(relations[i]);
                                }
                            }
                        }
                        catch { }

                        if (dataRow == null)
                        {
                            dataRow = row;
                        }

                        var dtrow = dataRow.Table.Columns.Contains(column.ColumnName) ? dataRow[column.ColumnName] : null;

                        if (this.XmlRefValue != null && this.XmlRefValue.ContainsKey(dtrow))
                        {
                            datas.Add(this.XmlRefValue[dtrow]);
                        }
                        else
                        {
                            datas.Add(dtrow);
                        }
                    }

                    dataSourceTable.Rows.Add(datas.ToArray());
                }
            }

            return dataSourceTable;
        }

        public override object GetQuerySchema(out string error)
        {
            try
            {
                error = string.Empty;
                return this.GetTableData();
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }
    }
}
