using System.Xml.Schema;

namespace BoldReports.Xml.Base.Schema
{
    class XMLSchemaInfoUtilities
    {
        /// <summary>
        /// Get all the schema info for the xml schema string.
        /// </summary>
        /// <param name="schemaElement">It contains the entire xml data</param>
        /// <returns>It returns the </returns>
        public static XMLSchemaInfo GetAllXmlSchemaInfo(XmlSchemaElement schemaElement)
        {
            XMLSchemaInfo xmlSchema = new XMLSchemaInfo();
            xmlSchema.SchemaName = schemaElement.Name;
            xmlSchema.SchemaType = GetCustomSchemaType(schemaElement.SchemaType);

            if (schemaElement?.SchemaType is XmlSchemaSimpleType)
            {
                XMLSchemaInfo xmlSimpleSchema = GetSimpleSchemaInfo(schemaElement);
                xmlSchema.ChildSchemas.Add(xmlSimpleSchema);
            }

            else if (schemaElement?.SchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexType = (XmlSchemaComplexType)schemaElement.ElementSchemaType;

                if (complexType?.Attributes != null)
                {
                    XMLSchemaInfo xmlAttributeSchema = GetComplextAttributeSchemaInfo(complexType.Attributes);
                    foreach (XMLSchemaInfo info in xmlAttributeSchema.ChildSchemas)
                    {
                        xmlSchema.SchemaType = GetCustomSchemaType(schemaElement.SchemaType, true);
                        xmlSchema.ChildSchemas.Add(info);
                    }
                }

                if (complexType?.Attributes?.Count == 0 && complexType?.AttributeUses != null)
                {
                    XMLSchemaInfo xmlAttributeSchema = GetComplextAttributeUsesSchemaInfo(complexType.AttributeUses);
                    foreach (XMLSchemaInfo info in xmlAttributeSchema.ChildSchemas)
                    {
                        xmlSchema.SchemaType = GetCustomSchemaType(schemaElement.SchemaType, true);
                        xmlSchema.ChildSchemas.Add(info);
                    }
                }

                if (complexType?.ContentTypeParticle != null && complexType?.ContentTypeParticle is XmlSchemaSequence)
                {
                    XMLSchemaInfo xmlParticleSchema = GetComplexParticleSchemaInfo(complexType);

                    foreach (XMLSchemaInfo info in xmlParticleSchema.ChildSchemas)
                    {
                        xmlSchema.ChildSchemas.Add(info);
                    }
                }
            }

            return xmlSchema;
        }

        /// <summary>
        /// Get XML schema infor for the XML particles.
        /// </summary>
        /// <param name="complexType">it defines the XmlSchemaComplexType as xml particle</param>
        /// <returns></returns>
        static XMLSchemaInfo GetComplexParticleSchemaInfo(XmlSchemaComplexType complexType)
        {

            XMLSchemaInfo xmlComplexParticleSchema = new XMLSchemaInfo();
            XMLSchemaInfo complexSequenceSchema = GetComplexSequenceSchemaInfo((XmlSchemaSequence)complexType.ContentTypeParticle);

            foreach (XMLSchemaInfo info in complexSequenceSchema.ChildSchemas)
            {
                xmlComplexParticleSchema.ChildSchemas.Add(info);
            }

            return xmlComplexParticleSchema;
        }

        /// <summary>
        /// Get the XML schema infor for the XML Complex Sequence.
        /// </summary>
        /// <param name="schemaSequence"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetComplexSequenceSchemaInfo(XmlSchemaSequence schemaSequence)
        {
            XMLSchemaInfo mainSchemaInfo = new XMLSchemaInfo();
            foreach (XmlSchemaObject innerentry in schemaSequence.Items)
            {
                XMLSchemaInfo innerSchemaInfo = new XMLSchemaInfo();

                if (innerentry is XmlSchemaElement)
                {
                    innerSchemaInfo = GetElementSchemaInfo(innerentry as XmlSchemaElement);
                }
                else if(innerentry is XmlSchemaChoice)
                {
                    innerSchemaInfo = GetSchemaChoiceSchemaInfo(innerentry as XmlSchemaChoice);
                }

                if (string.IsNullOrEmpty(innerSchemaInfo.SchemaName))
                {
                    foreach (XMLSchemaInfo schema in innerSchemaInfo.ChildSchemas)
                    {
                        mainSchemaInfo.ChildSchemas.Add(schema);
                    }
                }
                else
                {
                    mainSchemaInfo.ChildSchemas.Add(innerSchemaInfo);
                }
            }

            return mainSchemaInfo;
        }

        /// <summary>
        /// Get the schema for the Xml schema element.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetElementSchemaInfo(XmlSchemaElement entry)
        {
            XMLSchemaInfo innerSchemaInfo = new XMLSchemaInfo();
            if (entry.ElementSchemaType is XmlSchemaSimpleType)
            {
                innerSchemaInfo.ChildSchemas.Add(GetSimpleSchemaInfo(entry));
            }

            else if (entry.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexSubType = entry.ElementSchemaType as XmlSchemaComplexType;

                if (complexSubType?.ContentTypeParticle != null && complexSubType?.ContentTypeParticle is XmlSchemaSequence)
                {
                    innerSchemaInfo.SchemaName = entry.Name;
                    innerSchemaInfo.SchemaType = GetCustomSchemaType(entry.SchemaType);

                    XMLSchemaInfo xmlInfo = GetComplexParticleSchemaInfo(complexSubType);

                    foreach (XMLSchemaInfo schema in xmlInfo.ChildSchemas)
                    {
                        innerSchemaInfo.ChildSchemas.Add(schema);
                    }
                }

                if (complexSubType?.Attributes != null)
                {
                    innerSchemaInfo.SchemaName = entry.Name;
                    innerSchemaInfo.SchemaType = GetCustomSchemaType(entry.SchemaType);

                    XmlSchemaObjectCollection rootseq = complexSubType.Attributes;
                    XMLSchemaInfo xmlInfo = GetComplextAttributeSchemaInfo(rootseq);

                    foreach (XMLSchemaInfo schema in xmlInfo.ChildSchemas)
                    {
                        innerSchemaInfo.SchemaType = GetCustomSchemaType(entry.SchemaType, true);
                        innerSchemaInfo.ChildSchemas.Add(schema);
                    }
                }

                if (complexSubType?.Attributes?.Count == 0 && complexSubType?.AttributeUses != null)
                {
                    XmlSchemaObjectTable rootseq = complexSubType.AttributeUses;
                    XMLSchemaInfo xmlAttributeSchema = GetComplextAttributeUsesSchemaInfo(rootseq);

                    foreach (XMLSchemaInfo info in xmlAttributeSchema.ChildSchemas)
                    {
                        innerSchemaInfo.SchemaType = GetCustomSchemaType(entry.SchemaType, true);
                        innerSchemaInfo.ChildSchemas.Add(info);
                    }
                }

                if (entry.Name == null && entry.QualifiedName.Name != null)
                {
                    innerSchemaInfo.SchemaName = entry.QualifiedName.Name;
                    innerSchemaInfo.SchemaType = GetCustomSchemaType(entry.ElementSchemaType);
                }
            }

            return innerSchemaInfo;
        }

        /// <summary>
        /// Get the  XML schema infor for the XML Schema Choice.
        /// </summary>
        /// <param name="schemaChoice"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetSchemaChoiceSchemaInfo(XmlSchemaChoice schemaChoice)
        {
            XMLSchemaInfo choiceSchemaInfo = new XMLSchemaInfo();
            foreach (XmlSchemaElement choice in schemaChoice.Items)
            {
                XMLSchemaInfo SchemaInfo = GetElementSchemaInfo(choice);

                if (string.IsNullOrEmpty(SchemaInfo.SchemaName))
                {
                    foreach (XMLSchemaInfo schema in SchemaInfo.ChildSchemas)
                    {
                        choiceSchemaInfo.ChildSchemas.Add(schema);
                    }
                }
                else
                {
                    choiceSchemaInfo.ChildSchemas.Add(SchemaInfo);
                }
            }

            return choiceSchemaInfo;
        }

        /// <summary>
        /// Get the XML Schema info for xml simple schema type.
        /// </summary>
        /// <param name="schemaElement"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetSimpleSchemaInfo(XmlSchemaElement schemaElement)
        {
            XMLSchemaInfo schema = new XMLSchemaInfo();
            schema.SchemaName = schemaElement.Name == null ? schemaElement.QualifiedName.Name : schemaElement.Name;
            schema.SchemaType = GetCustomSchemaType(schemaElement.ElementSchemaType);
            if (!string.IsNullOrEmpty(schemaElement.SchemaTypeName.Name))
            {
                schema.ValueType = GetSchemaValueType(schemaElement.SchemaTypeName.Name);
            }
            else
            {
                schema.ValueType = GetSchemaValueType(schemaElement.ElementSchemaType.Datatype?.TypeCode.ToString());
            }

            return schema;
        }

        /// <summary>
        /// Get the schema info for the complex xml attibutes.
        /// </summary>
        /// <param name="schemaCollection"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetComplextAttributeSchemaInfo(XmlSchemaObjectCollection schemaCollection)
        {
            XMLSchemaInfo schema = new XMLSchemaInfo();
            foreach (XmlSchemaAttribute innerentry in schemaCollection)
            {
                if (innerentry.AttributeSchemaType.Datatype.TypeCode != XmlTypeCode.AnyUri)
                {
                    XMLSchemaInfo info = new XMLSchemaInfo
                    {
                        SchemaName = innerentry.Name,
                        SchemaType = GetCustomSchemaType(innerentry.AttributeSchemaType),
                        ValueType = GetSchemaValueType(innerentry.AttributeSchemaType.Datatype.ValueType.Name)
                    };
                    schema.ChildSchemas.Add(info);
                }
            }

            return schema;
        }

        /// <summary>
        /// Get the schema info for the Complex attribute uses.
        /// </summary>
        /// <param name="schemaCollection"></param>
        /// <returns></returns>
        static XMLSchemaInfo GetComplextAttributeUsesSchemaInfo(XmlSchemaObjectTable schemaCollection)
        {
            XMLSchemaInfo schema = new XMLSchemaInfo();
            foreach (XmlSchemaAttribute innerentry in schemaCollection.Values)
            {
                if (innerentry.AttributeSchemaType.Datatype.TypeCode != XmlTypeCode.AnyUri)
                {
                    XMLSchemaInfo info = new XMLSchemaInfo
                    {
                        SchemaName = innerentry.Name,
                        SchemaType = GetCustomSchemaType(innerentry.AttributeSchemaType),
                        ValueType = GetSchemaValueType(innerentry.AttributeSchemaType.Datatype.ValueType.Name)
                    };
                    schema.ChildSchemas.Add(info);
                }
            }

            return schema;
        }

        /// <summary>
        /// Get the value for the Schema type for XML schema info(XML Element).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isSequenceArray"></param>
        /// <returns></returns>
        internal static XMLCustomSchemaType GetCustomSchemaType(XmlSchemaType type, bool isSequenceArray = false)
        {
            XMLCustomSchemaType schemaType = new XMLCustomSchemaType();

            if (type is XmlSchemaSimpleType)
            {
                schemaType = XMLCustomSchemaType.XmlSchemaSimpleType;
            }
            else if (type is XmlSchemaComplexType && !isSequenceArray)
            {
                schemaType = XMLCustomSchemaType.XmlSchemaComplexType;
            }
            else if (type is XmlSchemaComplexType && isSequenceArray)
            {
                schemaType = XMLCustomSchemaType.XmlSchemaArrayType;
            }

            return schemaType;
        }

        /// <summary>
        /// Get the value type for the XML Schema Elelemnt.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static XMLCustomValueType GetSchemaValueType(string type)
        {
            XMLCustomValueType xmlCustomValueType = XMLCustomValueType.String;
            switch (type.ToLower())
            {
                case "string":
                    xmlCustomValueType = XMLCustomValueType.String;
                    break;
                case "decimal":
                    xmlCustomValueType = XMLCustomValueType.Decimal;
                    break;
                case "date":
                    xmlCustomValueType = XMLCustomValueType.date;
                    break;
                case "unsignedshort":
                    xmlCustomValueType = XMLCustomValueType.unsignedShort;
                    break;
                case "time":
                    xmlCustomValueType = XMLCustomValueType.time;
                    break;
                case "duration":
                    xmlCustomValueType = XMLCustomValueType.duration;
                    break;
                case "datetime":
                    xmlCustomValueType = XMLCustomValueType.dateTime;
                    break;
                case "uri":
                    xmlCustomValueType = XMLCustomValueType.anyUri;
                    break;
                case "unsignedbyte":
                case "signedbyte":
                case "byte":
                case "unsignedint":
                case "signedint":
                case "int":
                    xmlCustomValueType = XMLCustomValueType.integer;
                    break;
            }

            return xmlCustomValueType;
        }
    }
}
