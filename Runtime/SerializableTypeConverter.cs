using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NeroWeNeed.Commons {
    public class SerializableTypeConverter : JsonConverter<SerializableType> {
        public override SerializableType ReadJson(JsonReader reader, Type objectType, SerializableType existingValue, bool hasExistingValue, JsonSerializer serializer) {
            /*             var assemblyQualifiedName = (string)reader.Value;
                        if (hasExistingValue) {
                            existingValue.assemblyQualifiedName = assemblyQualifiedName;
                            return existingValue;
                        }
                        else {
                            return new SerializableType(assemblyQualifiedName);
                        } */
            if (reader.TokenType == JsonToken.StartObject) {
                JObject type = JObject.Load(reader);
                var baseType = type["type"].Value<string>();
                var genericArguments = type["arguments"].Values<string>();
                if (hasExistingValue) {
                    existingValue.assemblyQualifiedName = baseType;
                    existingValue.genericArguments = genericArguments.ToArray();
                    return existingValue;
                }
                else {
                    return new SerializableType(existingValue.assemblyQualifiedName, genericArguments.ToArray());
                }
            }
            else if (reader.TokenType == JsonToken.String) {
                var assemblyQualifiedName = (string)reader.Value;
                if (hasExistingValue) {
                    existingValue.assemblyQualifiedName = assemblyQualifiedName;
                    return existingValue;
                }
                else {
                    return new SerializableType(assemblyQualifiedName);
                }
            }
            else if (reader.TokenType == JsonToken.Null) {
                if (hasExistingValue) {
                    return existingValue;
                }
                else {
                    return new SerializableType();
                }
            }
            else {
                throw new JsonException();
            }
        }
        public override void WriteJson(JsonWriter writer, SerializableType value, JsonSerializer serializer) {
            if (value.IsGenericType) {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue(value.AssemblyQualifiedName);
                writer.WritePropertyName("arguments");
                writer.WriteStartArray();
                foreach (var genericArgument in value.genericArguments) {
                    writer.WriteValue(genericArgument);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else {
                writer.WriteValue(value.AssemblyQualifiedName);
            }
            /*             if (value.IsConstructedGeneric) {
                            writer.WriteStartObject();
                            writer.WritePropertyName("type");
                            writer.WriteValue(value.AssemblyQualifiedName);
                            writer.WritePropertyName("arguments");
                            writer.WriteStartArray();
                            foreach (var genericArgument in value.genericArguments) {
                                writer.WriteValue(genericArgument);
                            }
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                        }
                        else {
                            writer.WriteValue(value.AssemblyQualifiedName);
                        }
             */
        }
    }



}