using BoldReports.WebDatasource.Base.Model;
using BoldReports.Xml.Base.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoldReports.Web.DataProviders.Helper
{
    internal class XmlHelper
    {
        /// <summary>
        /// Get the XML schema info details of the xml data/ xml data source
        /// </summary>
        /// <param name="selectedTableSchema"></param>
        /// <returns></returns>
        internal static ObservableCollection<XMLSchemaInfo> PortSelectedTableToXmlSchemInfo(List<TableSchemaInfo> selectedTableSchema)
        {
            ObservableCollection<XMLSchemaInfo> selectedXmlSchemas = new ObservableCollection<XMLSchemaInfo>();
            selectedTableSchema.ForEach(tableSchema =>
            {
                var schema = new XMLSchemaInfo
                {
                    SchemaName = tableSchema.ColumnName,
                    SchemaType = tableSchema.XmlSchemaType,
                    IsAnonymousSchema = tableSchema.IsAnonymousSchema,
                    ValueType = tableSchema.XmlValueType
                };

                var childSchemas = RemoveXmlSchemaFromOriginalSchema(tableSchema.ColumnSchemaInfoCollection);
                foreach (var s in childSchemas)
                {
                    schema.ChildSchemas.Add(s);
                }
                selectedXmlSchemas.Add(schema);
            });
            return selectedXmlSchemas;
        }

        /// <summary>
        /// Removes the selected the XML schema from the original xml schema.
        /// </summary>
        /// <param name="selectedTableSchema"></param>
        /// <returns></returns>
        private static ObservableCollection<XMLSchemaInfo> RemoveXmlSchemaFromOriginalSchema(List<TableSchemaInfo> selectedTableSchema)
        {
            ObservableCollection<XMLSchemaInfo> selectedXmlSchemas = new ObservableCollection<XMLSchemaInfo>();
            if (selectedTableSchema == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var tableSchema in selectedTableSchema)
            {
                XMLSchemaInfo schema = new XMLSchemaInfo()
                {
                    SchemaName = tableSchema.ColumnName,
                    IsAnonymousSchema = tableSchema.IsAnonymousSchema,
                    SchemaType = tableSchema.XmlSchemaType,
                    ValueType = tableSchema.XmlValueType,
                };
                foreach (XMLSchemaInfo item in RemoveXmlSchemaFromOriginalSchema(tableSchema.ColumnSchemaInfoCollection))
                {
                    schema.ChildSchemas.Add(item);
                }
                selectedXmlSchemas.Add(schema);
            }
            return selectedXmlSchemas;
        }

    }
}
