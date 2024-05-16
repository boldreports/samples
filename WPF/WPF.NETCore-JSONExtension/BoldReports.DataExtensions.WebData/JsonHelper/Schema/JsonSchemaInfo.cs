using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.ObjectModel;

namespace BoldReports.Json.Base.Schema
{
    public enum JsonCustomSchemaType
    {
        None,

        Object,

        Array,

        Null,

        Value
    }

    public enum JsonCustomValueType
    {
        Object,
    
        String,

        Number,

        Boolean,

        DateTime,

        DateTimeOffset,

        Bytes,

        Guid,

        Uri
    }

    public class JsonSchemaInfo : ICloneable
    {
        private readonly ObservableCollection<JsonSchemaInfo> childSchemas = new ObservableCollection<JsonSchemaInfo>();

        public bool IsAnonymousSchema { get; set; }

        public string SchemaName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JsonCustomSchemaType SchemaType { get; set; }

        public int InnerArrayCount { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JsonCustomSchemaType FiniteArraySchemaType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public JsonCustomValueType ValueType { get; set; }

        [JsonIgnore]
        public bool HasChild { get { return this.childSchemas.Count > 0; } }

        public bool IsMapping { get; set; }
        public ObservableCollection<JsonSchemaInfo> ChildSchemas
        {
            get { return this.childSchemas; }
        }

        public object Clone()
        {
            JsonSchemaInfo jsonSchemaInfo = new JsonSchemaInfo()
            {
                SchemaName = this.SchemaName,
                SchemaType = this.SchemaType,
                InnerArrayCount = this.InnerArrayCount,
                FiniteArraySchemaType = this.FiniteArraySchemaType,
                ValueType = this.ValueType,
                IsAnonymousSchema = this.IsAnonymousSchema,
                IsMapping = this.IsMapping
            };

            foreach(JsonSchemaInfo childSchema in this.ChildSchemas)
            {
                jsonSchemaInfo.ChildSchemas.Add(childSchema.Clone() as JsonSchemaInfo);
            }

            return jsonSchemaInfo;
        }

        public bool IsEqualSchema(JsonSchemaInfo jsonSchemaInfo)
        {
            return jsonSchemaInfo != null &&
                jsonSchemaInfo.SchemaName == this.SchemaName &&
                jsonSchemaInfo.SchemaType == this.SchemaType &&
                jsonSchemaInfo.InnerArrayCount == this.InnerArrayCount &&
                jsonSchemaInfo.FiniteArraySchemaType == this.FiniteArraySchemaType &&
                jsonSchemaInfo.ValueType == this.ValueType &&
                jsonSchemaInfo.HasChild == this.HasChild;
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(this.SchemaName) ? this.SchemaName : base.ToString();
        }
    }
}
