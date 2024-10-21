using System;
using System.Collections.ObjectModel;

namespace BoldReports.Xml.Base.Schema
{
    public class XMLSchemaInfo : ICloneable
    {
        private readonly ObservableCollection<XMLSchemaInfo> childSchemas = new ObservableCollection<XMLSchemaInfo>();

        public bool IsAnonymousSchema { get; set; }

        public string SchemaName { get; set; }

        public XMLCustomSchemaType SchemaType { get; set; }

        public XMLCustomValueType ValueType { get; set; }

        public ObservableCollection<XMLSchemaInfo> ChildSchemas
        {
            get { return this.childSchemas; }
        }

        public bool HasChild { get { return this.childSchemas.Count > 0; } }

        public object Clone()
        {
            XMLSchemaInfo xmlSchemaInfo = new XMLSchemaInfo()
            {
                SchemaName = this.SchemaName,
                IsAnonymousSchema = this.IsAnonymousSchema,
            };

            foreach (XMLSchemaInfo childSchema in this.ChildSchemas)
            {
                xmlSchemaInfo.ChildSchemas.Add(childSchema.Clone() as XMLSchemaInfo);
            }

            return xmlSchemaInfo;
        }

        public bool IsEqualSchema(XMLSchemaInfo xmlSchemaInfo)
        {
            return xmlSchemaInfo != null &&
                xmlSchemaInfo.SchemaName == this.SchemaName &&
                xmlSchemaInfo.HasChild == this.HasChild;
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(this.SchemaName) ? this.SchemaName : base.ToString();
        }
    }


    public enum XMLCustomSchemaType
    {
        XmlSchemaSimpleType,

        XmlSchemaArrayType,

        XmlSchemaComplexType,

        XmlSchemaNullType
    }


    public enum XMLCustomValueType
    {
        String,
        Byte,
        hexBinary,
        base64Binary,
        boolean,
        normalizedString,
        date,
        dateTime,
        Decimal,
        Double,
        duration,
        entities,
        entity,
        Float,
        gMonthDay,
        gDay,
        gYear,
        gYearMonth,
        id,
        idRef,
        idRefs,
        Int,
        integer,
        language,
        Long,
        month,
        name,
        nCName,
        negativeInteger,
        nMToken,
        nMTokens,
        nonNegativeInteger,
        nonPositiveInteger,
        notation,
        positiveInteger,
        qName,
        Short,
        time,
        timePeriod,
        token,
        unsignedByte,
        unsignedInt,
        unsignedLong,
        unsignedShort,
        anyUri
    }
}
