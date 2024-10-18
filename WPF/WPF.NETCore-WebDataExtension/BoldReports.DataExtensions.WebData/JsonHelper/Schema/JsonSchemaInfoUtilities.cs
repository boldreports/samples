using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BoldReports.Json.Base.Schema
{
    internal static class JsonSchemaInfoUtilities
    {
        internal static JsonCustomSchemaType GetSchemaType(JTokenType type)
        {
            JsonCustomSchemaType jsonCustomSchemaType = JsonCustomSchemaType.None;
            switch (type)
            {
                case JTokenType.Object:
                    jsonCustomSchemaType = JsonCustomSchemaType.Object;
                    break;

                case JTokenType.Array:
                    jsonCustomSchemaType = JsonCustomSchemaType.Array;
                    break;

                case JTokenType.Null:
                    jsonCustomSchemaType = JsonCustomSchemaType.Null;
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                    jsonCustomSchemaType = JsonCustomSchemaType.Value;
                    break;
            }

            return jsonCustomSchemaType;
        }

        internal static JsonCustomValueType GetSchemaValueType(JTokenType type)
        {
            JsonCustomValueType jsonCustomSchemaType = JsonCustomValueType.Object;
            switch (type)
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    jsonCustomSchemaType = JsonCustomValueType.Number;
                    break;
                case JTokenType.String:
                    jsonCustomSchemaType = JsonCustomValueType.String;
                    break;
                case JTokenType.Boolean:
                    jsonCustomSchemaType = JsonCustomValueType.Boolean;
                    break;
                case JTokenType.Date:
                    jsonCustomSchemaType = JsonCustomValueType.DateTime;
                    break;
                case JTokenType.Bytes:
                    jsonCustomSchemaType = JsonCustomValueType.Bytes;
                    break;
                case JTokenType.Guid:
                    jsonCustomSchemaType = JsonCustomValueType.Guid;
                    break;
                case JTokenType.Uri:
                    jsonCustomSchemaType = JsonCustomValueType.Uri;
                    break;
                case JTokenType.TimeSpan:
                    jsonCustomSchemaType = JsonCustomValueType.DateTimeOffset;
                    break;

                case JTokenType.Undefined:
                case JTokenType.Raw:
                    jsonCustomSchemaType = JsonCustomValueType.Object;
                    break;

            }
            return jsonCustomSchemaType;
        }       

        internal static ObservableCollection<JsonSchemaInfo> GetObjectJsonSchemas(JObject jObject, bool isMapping = false)
        {
            ObservableCollection<JsonSchemaInfo> jsonCustomSchemaCollection = new ObservableCollection<JsonSchemaInfo>();
            foreach (KeyValuePair<string, JToken> item in jObject)
            {
                JsonSchemaInfo tempJsonObjectSchemaInfo = GetObjectJsonSchema(item.Key, item.Value,isMapping);
                if (tempJsonObjectSchemaInfo != null)
                {
                    jsonCustomSchemaCollection.Add(tempJsonObjectSchemaInfo);
                }
            }

            return jsonCustomSchemaCollection;
        }

        internal static JsonSchemaInfo GetObjectJsonSchema(string key, JToken value,bool isMapping = false)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
            {
                return null;
            }

            JsonSchemaInfo jsonSchemaInfo = new JsonSchemaInfo
            {
                SchemaName = key,
                SchemaType = JsonSchemaInfoUtilities.GetSchemaType(value.Type),
                IsMapping = isMapping
            };
            
            switch (jsonSchemaInfo.SchemaType)
            {
                case JsonCustomSchemaType.None:
                    jsonSchemaInfo = null;
                    break;

                case JsonCustomSchemaType.Object:
                    ObservableCollection<JsonSchemaInfo> objectJsonSchemas = GetObjectJsonSchemas(value as JObject);
                    foreach (JsonSchemaInfo objectJsonSchema in objectJsonSchemas)
                    {
                        jsonSchemaInfo.ChildSchemas.Add(objectJsonSchema);
                    }

                    break;

                case JsonCustomSchemaType.Array:
                    JsonSchemaInfoUtilities.SetArrayInfo(jsonSchemaInfo, value);
                    break;

                case JsonCustomSchemaType.Value:
                    jsonSchemaInfo.ValueType = JsonSchemaInfoUtilities.GetSchemaValueType(value.Type);
                    break;
            }

            return jsonSchemaInfo;
        }

        private static void GetSchemaUsingArrayItem(ref ObservableCollection<JsonSchemaInfo> resultJsonSchemas, ObservableCollection<JsonSchemaInfo> jsonSchemas)
        {
            bool hasItemInResultCollection = resultJsonSchemas != null && resultJsonSchemas.Count > 0;
            bool hasItemInMergeCollection = jsonSchemas != null && jsonSchemas.Count > 0;
            if (hasItemInResultCollection && hasItemInMergeCollection)
            {
                foreach (JsonSchemaInfo tempJsonSchema in jsonSchemas)
                {
                    JsonSchemaInfo actualJsonSchmea = resultJsonSchemas.FirstOrDefault(item => item.SchemaName == tempJsonSchema.SchemaName);
                    if (actualJsonSchmea != null)
                    {
                        if (actualJsonSchmea.SchemaType == tempJsonSchema.SchemaType && 
                            actualJsonSchmea.FiniteArraySchemaType == tempJsonSchema.FiniteArraySchemaType
                            && actualJsonSchmea.InnerArrayCount == tempJsonSchema.InnerArrayCount)
                        {
                            JsonCustomSchemaType tempSchemaType = actualJsonSchmea.SchemaType == JsonCustomSchemaType.Array ? 
                                actualJsonSchmea.FiniteArraySchemaType : actualJsonSchmea.SchemaType;
                            switch (tempSchemaType)
                            {
                                case JsonCustomSchemaType.Object:
                                    ObservableCollection<JsonSchemaInfo> result = new ObservableCollection<JsonSchemaInfo>(actualJsonSchmea.ChildSchemas);
                                    actualJsonSchmea.ChildSchemas.Clear();
                                    GetSchemaUsingArrayItem(ref result, tempJsonSchema.ChildSchemas);
                                    if (result != null)
                                    {
                                        foreach (JsonSchemaInfo tempObjectSchema in result)
                                        {
                                            actualJsonSchmea.ChildSchemas.Add(tempObjectSchema);
                                        }
                                    }

                                    break;

                                case JsonCustomSchemaType.Value:
                                    if (actualJsonSchmea.ValueType == JsonCustomValueType.Object && actualJsonSchmea.ValueType != tempJsonSchema.ValueType)
                                    {
                                        actualJsonSchmea.ValueType = tempJsonSchema.ValueType;
                                    }
                                    break;
                            }
                        }
                        else if ((actualJsonSchmea.SchemaType == JsonCustomSchemaType.None && tempJsonSchema.SchemaType != JsonCustomSchemaType.None) ||
                            (actualJsonSchmea.SchemaType == JsonCustomSchemaType.Null && tempJsonSchema.SchemaType != JsonCustomSchemaType.Null && tempJsonSchema.SchemaType != JsonCustomSchemaType.None) ||
                            (actualJsonSchmea.SchemaType == JsonCustomSchemaType.Array && actualJsonSchmea.FiniteArraySchemaType == JsonCustomSchemaType.None &&
                            tempJsonSchema.SchemaType == JsonCustomSchemaType.Array && tempJsonSchema.FiniteArraySchemaType != JsonCustomSchemaType.None) ||
                            (actualJsonSchmea.SchemaType == JsonCustomSchemaType.Array && actualJsonSchmea.FiniteArraySchemaType == JsonCustomSchemaType.Null &&
                            tempJsonSchema.SchemaType == JsonCustomSchemaType.Array && tempJsonSchema.FiniteArraySchemaType != JsonCustomSchemaType.Null && tempJsonSchema.FiniteArraySchemaType != JsonCustomSchemaType.None))
                        {
                            int index = resultJsonSchemas.IndexOf(actualJsonSchmea);
                            resultJsonSchemas[index] = tempJsonSchema;
                        }
                    }
                    else
                    {
                        resultJsonSchemas.Add(tempJsonSchema);
                    }
                }
            }
            else if (hasItemInMergeCollection)
            {
                resultJsonSchemas = new ObservableCollection<JsonSchemaInfo>(jsonSchemas);
            }
        }

        private static void SetFiniteArrayInfo(JsonSchemaInfo jsonSchemaInfo, IList<JToken> value)
        {
            int currentDepth = 0;
            SetFiniteArrayInfo(jsonSchemaInfo, value, ref currentDepth);
        }


        private static void SetFiniteArrayInfo(JsonSchemaInfo jsonSchemaInfo, IList<JToken> arrayTokens, ref int currentDepth)
        {
            if (arrayTokens != null)
            {
                foreach (JToken arrayToken in arrayTokens)
                {
                    switch (GetSchemaType(arrayToken.Type))
                    {
                        case JsonCustomSchemaType.Array:
                            ++currentDepth;
                            SetFiniteArrayInfo(jsonSchemaInfo, arrayToken as IList<JToken>, ref currentDepth);
                            --currentDepth;
                            break;

                        case JsonCustomSchemaType.Null:
                            if (jsonSchemaInfo.InnerArrayCount <= currentDepth)
                            {
                                jsonSchemaInfo.InnerArrayCount = currentDepth;
                                jsonSchemaInfo.FiniteArraySchemaType = JsonCustomSchemaType.Null; 
                            }
                           
                            break;

                        case JsonCustomSchemaType.Object:
                            if (jsonSchemaInfo.InnerArrayCount <= currentDepth)
                            {
                                jsonSchemaInfo.InnerArrayCount = currentDepth;
                                jsonSchemaInfo.FiniteArraySchemaType = JsonCustomSchemaType.Object;
                                return;
                            }

                            break;
                            

                        case JsonCustomSchemaType.Value:
                            if (jsonSchemaInfo.InnerArrayCount <= currentDepth)
                            {
                                jsonSchemaInfo.InnerArrayCount = currentDepth;
                                jsonSchemaInfo.FiniteArraySchemaType = JsonCustomSchemaType.Value;
                                jsonSchemaInfo.ValueType = GetSchemaValueType(arrayToken.Type);
                                return;
                            }

                            break;
                    }
                }
            }
        }

        internal static void SetArrayInfo(JsonSchemaInfo jsonSchemaInfo, JToken value)
        {
            IList<JToken> arrayValues = value as IList<JToken>;
            if (arrayValues != null && arrayValues.Count > 0)
            {
                SetFiniteArrayInfo(jsonSchemaInfo, arrayValues);
                if (jsonSchemaInfo.FiniteArraySchemaType == JsonCustomSchemaType.Object)
                {
                    arrayValues = JsonTabularUtilities.GetArrayItems(arrayValues, jsonSchemaInfo.InnerArrayCount);
                    ObservableCollection<JsonSchemaInfo> result = new ObservableCollection<JsonSchemaInfo>(jsonSchemaInfo.ChildSchemas);
                    jsonSchemaInfo.ChildSchemas.Clear();
                    foreach (JToken arrayValue in arrayValues)
                    {
                        JObject tempjObject = arrayValue as JObject;
                        if (tempjObject != null)
                        {
                            ObservableCollection<JsonSchemaInfo> arrayElementResult = GetObjectJsonSchemas(tempjObject);
                            GetSchemaUsingArrayItem(ref result, arrayElementResult);
                        }
                    }

                    if (result != null)
                    {
                        foreach (JsonSchemaInfo tempObjectSchema in result)
                        {
                            jsonSchemaInfo.ChildSchemas.Add(tempObjectSchema);
                        }
                    }
                }
            }
        }
    }
}
