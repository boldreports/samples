using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using BoldReports.Json.Base.Schema;

namespace BoldReports.Json.Base
{
    internal enum MergeType
    {
        Matrix,

        Row
    }

    internal static class JsonTabularUtilities
    {
        internal static void TrimDataColumnNames(DataTable table, List<string> columns, bool IsMapping = false)
        {
            if (table != null)
            {
                List<CustomColumnNameInfo> customNames = new List<CustomColumnNameInfo>();
                foreach (DataColumn dataColumn in table.Columns)
                {
                    customNames.Add(new CustomColumnNameInfo(dataColumn.ColumnName));
                }

                IsMapping = false;
                if (!IsMapping)
                {
                    foreach (CustomColumnNameInfo customName in customNames)
                    {
                        CustomColumnNameInfo[] tempDuplicateNames = customNames.Where(tempName => tempName.ColumnName.ToLower() == customName.ColumnName.ToLower()).ToArray();

                        if (tempDuplicateNames.Length <= 1)
                        {
                            if(!(columns != null && columns.Count > 0 && tempDuplicateNames.Length == 1 && columns.Where(t => t.ToLower() == customName.ColumnName.ToLower()).Count() > 1))
                                continue;
                        }

                        foreach (CustomColumnNameInfo customColumnNameInfo in tempDuplicateNames)
                        {
                            customColumnNameInfo.PrepareSimplifiedUniqueColumnName();
                        }
                    }
                }
                else
                {
                    foreach (CustomColumnNameInfo customName in customNames)
                    {
                        customName.PrepareSimplifiedUniqueColumnName();
                    }
                }

                foreach (CustomColumnNameInfo customName in customNames)
                {
                    DataColumn column = table.Columns[customName.ColumnFullName];
                    column.ColumnName = customName.ColumnName;
                }
            }
        }

        private static string GetColumnName(List<string> schemaNames)
        {
            string columnName = string.Empty;
            if (schemaNames != null && schemaNames.Count > 0)
            {
                schemaNames.ForEach(key => columnName = columnName.Length == 0 ?
                    JsonProperties.LeftDelimiter + key + JsonProperties.RightDelimiter :
                    columnName + JsonProperties.GetUniqueSplitter() + JsonProperties.LeftDelimiter + key + JsonProperties.RightDelimiter);
            }
            return columnName;
        }

        internal static DataTable GetJsonTable(ObservableCollection<JsonSchemaInfo> selectedJsonSchemaInfos, JToken jToken)
        {
            JToken tempToken = jToken;
            if (selectedJsonSchemaInfos.Count == 1 && selectedJsonSchemaInfos.First().IsAnonymousSchema)
            {
                tempToken = new JObject {{JsonDataProvider.AnonymousArray, jToken}};
            }

            return ProcessJsonData(null, selectedJsonSchemaInfos, tempToken);
        }

        internal static Type GetDataType(JsonCustomValueType jsonType)
        {
            Type type = typeof(object);
            switch (jsonType)
            {
                case JsonCustomValueType.String:
                    type = typeof(string);
                    break;
                case JsonCustomValueType.Number:
                    type = typeof(double);
                    break;
                case JsonCustomValueType.Boolean:
                    type = typeof(bool);
                    break;
                case JsonCustomValueType.DateTime:
                    type = typeof(DateTime);
                    break;
                case JsonCustomValueType.DateTimeOffset:
                    type = typeof(DateTimeOffset);
                    break;
                case JsonCustomValueType.Bytes:
                    type = typeof(byte);
                    break;
                case JsonCustomValueType.Guid:
                    type = typeof(Guid);
                    break;
                case JsonCustomValueType.Uri:
                    type = typeof(Uri);
                    break;
            }

            return type;
        }

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
                        row[i] = i < firstTable.Columns.Count ? firstTableRow[i] : secondTableRow[i - firstTable.Columns.Count];
                    }

                    table.Rows.Add(row);
                }
            }

            ClearTableData(firstTable);
            ClearTableData(secondTable);
            return table;
        }

        private static DataTable RowMerge(DataTable firstTable, DataTable secondTable)
        {
            firstTable.Merge(secondTable, true, MissingSchemaAction.Add);
            ClearTableData(secondTable);
            return firstTable;
        }

        private static void ClearTableData(DataTable table)
        {
            if (table != null)
            {
                table.Rows.Clear();
                table.Columns.Clear();
                table.Dispose();
            }
        }

        internal static bool CanTryToGetValueUsingKey(JToken jToken)
        {
            return
                jToken != null &&
                jToken.Type != JTokenType.None &&
                jToken.Type != JTokenType.Null;
        }

        internal static JToken GetValueUsingKey(JToken jToken, string key)
        {
            JToken value = null;
            if (CanTryToGetValueUsingKey(jToken))
            {
                try
                {
                    value = jToken[key];
                }
                catch (Exception)
                {
                    value = null;
                }
            }

            return value;       
        }

        private static DataTable ProcessJsonData(ICollection<string> parentSchemaNames, IEnumerable<JsonSchemaInfo> selectedJsonSchemaInfos, JToken jToken)
        {
            DataTable table = null;
            foreach (JsonSchemaInfo jsonSchemaInfo in selectedJsonSchemaInfos)
            {
                JToken value = GetValueUsingKey(jToken, jsonSchemaInfo.SchemaName);
                List<string> tempSchemaNames = GetSchemaNames(parentSchemaNames, jsonSchemaInfo);
                DataTable schemaItemTable = null;
                switch (jsonSchemaInfo.SchemaType)
                {
                    case JsonCustomSchemaType.Object:
                        schemaItemTable = ProcessJsonData(tempSchemaNames, jsonSchemaInfo.ChildSchemas, value);
                        break;

                    case JsonCustomSchemaType.Array:
                        schemaItemTable = ProcessJsonArrayData(tempSchemaNames, jsonSchemaInfo, value);
                        break;

                    case JsonCustomSchemaType.Null:
                    {
                        string columnName = GetColumnName(tempSchemaNames);
                        schemaItemTable = new DataTable
                        {
                            Locale = CultureInfo.InvariantCulture
                        };
                        schemaItemTable.Columns.Add(columnName, GetDataType(jsonSchemaInfo.ValueType));
                        schemaItemTable.Rows.Add(schemaItemTable.NewRow());
                    }
                        break;

                    case JsonCustomSchemaType.Value:
                    {
                        string columnName = GetColumnName(tempSchemaNames);
                        schemaItemTable = new DataTable
                        {
                            Locale = CultureInfo.InvariantCulture
                        };
                        schemaItemTable.Columns.Add(columnName, GetDataType(jsonSchemaInfo.ValueType));
                        DataRow row = schemaItemTable.NewRow();
                        schemaItemTable.Rows.Add(row);

                        if (value != null && 
                                jsonSchemaInfo.SchemaType == JsonSchemaInfoUtilities.GetSchemaType(value.Type) && 
                                jsonSchemaInfo.ValueType == JsonSchemaInfoUtilities.GetSchemaValueType(value.Type))
                        {
                            row[columnName] = value;
                        }
                    }
                        break;
                }

                table = Merge(table, schemaItemTable, MergeType.Matrix);
            } 

            return table;
        }

        internal static IList<JToken> GetArrayItems(IEnumerable<JToken> arrayTokens, int finalDepth)
        {
            int initialCount = 0;
            return GetArrayItems(arrayTokens, ref initialCount, finalDepth);
        }

        private static IList<JToken> GetArrayItems(IEnumerable<JToken> arrayTokens, ref int currentDepth, int finalDepth)
        {
            List<JToken> resultArrayItems = new List<JToken>();
            if (arrayTokens != null && currentDepth > -1 && finalDepth > -1 && currentDepth <= finalDepth)
            {
                if (currentDepth == finalDepth)
                {
                    IList<JToken> tokens = arrayTokens as IList<JToken> ?? arrayTokens.ToList();
                    if (tokens.Count > 0)
                    {
                        resultArrayItems.AddRange(tokens);
                    }
                    else
                    {
                        resultArrayItems.Add(JValue.CreateNull());
                    }
                }
                else
                {
                    ++currentDepth;
                    foreach (JToken arrayToken in arrayTokens)
                    {
                        resultArrayItems.AddRange(GetArrayItems(arrayToken as IList<JToken>, ref currentDepth, finalDepth));
                    }

                    --currentDepth;
                }
            }
            else
            {
                resultArrayItems.Add(JValue.CreateNull());
            }

            return resultArrayItems;
        }

        private static DataTable ProcessJsonArrayData(List<string> tempSchemaNames, JsonSchemaInfo jsonSchemaInfo, JToken value)
        {
            DataTable resultTable = null;
            List<JToken> resultArrayItems = new List<JToken>();
            IList<JToken> finiteArrayTokens = value as IList<JToken>;
            if (finiteArrayTokens != null)
            {
                resultArrayItems.AddRange(GetArrayItems(finiteArrayTokens, jsonSchemaInfo.InnerArrayCount));
            }
            else
            {
                resultArrayItems.Add(JValue.CreateNull());
            }

            switch (jsonSchemaInfo.FiniteArraySchemaType)
            {
                case JsonCustomSchemaType.Object:
                    foreach (JToken arrayItem in resultArrayItems)
                    {
                        DataTable itemTable = null;
                        if (arrayItem != null && jsonSchemaInfo.FiniteArraySchemaType == JsonSchemaInfoUtilities.GetSchemaType(arrayItem.Type))
                        {
                            itemTable = ProcessJsonData(tempSchemaNames, jsonSchemaInfo.ChildSchemas, arrayItem);
                        }

                        resultTable = Merge(resultTable, itemTable, MergeType.Row);
                    }

                    break;

                case JsonCustomSchemaType.Null:
                {
                    string columnName = GetColumnName(tempSchemaNames);
                    resultTable = new DataTable
                    {
                        Locale = CultureInfo.InvariantCulture
                    };
                    resultTable.Columns.Add(columnName, GetDataType(jsonSchemaInfo.ValueType));
                    resultTable.Rows.Add(resultTable.NewRow());
                    for (int i = 1; i < resultArrayItems.Count; i++)
                    {
                        resultTable.Rows.Add(resultTable.NewRow());
                    }
                }

                    break;

                case JsonCustomSchemaType.Value:
                {
                    string columnName = GetColumnName(tempSchemaNames);
                    resultTable = new DataTable
                    {
                        Locale = CultureInfo.InvariantCulture
                    };
                    resultTable.Columns.Add(columnName, GetDataType(jsonSchemaInfo.ValueType));
                    foreach (JToken arrayItem in resultArrayItems)
                    {
                        DataRow row = resultTable.NewRow();
                        resultTable.Rows.Add(row);
                        if (arrayItem != null && 
                            jsonSchemaInfo.FiniteArraySchemaType == JsonSchemaInfoUtilities.GetSchemaType(arrayItem.Type) && 
                            jsonSchemaInfo.ValueType == JsonSchemaInfoUtilities.GetSchemaValueType(arrayItem.Type))
                        {
                            row[columnName] = arrayItem;
                        }
                    }
                }

                    break;
            }

            return resultTable;
        }

        private static List<string> GetSchemaNames(ICollection<string> parentSchemaNames, JsonSchemaInfo jsonSchemaInfo)
        {
            List<string> tempSchemaNames = new List<string>();
            if (parentSchemaNames != null)
            {
                tempSchemaNames.AddRange(parentSchemaNames);
            }

            if (!jsonSchemaInfo.IsAnonymousSchema)
            {
                tempSchemaNames.Add(jsonSchemaInfo.SchemaName);
            }

            return tempSchemaNames;
        }
    }
}
