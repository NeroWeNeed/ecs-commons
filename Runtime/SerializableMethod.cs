using System;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace NeroWeNeed.Commons {

    [Serializable]
    public struct SerializableMethod {
        public SerializableType container;
        public string name;
        [JsonIgnore]
        public bool IsCreated { get => container.IsCreated && !string.IsNullOrEmpty(name); }
        [JsonIgnore]
        public MethodInfo Value { get => container.Value?.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static); }
        public SerializableMethod(MethodInfo method) {
            container = new SerializableType(method?.DeclaringType);
            name = method?.Name;
        }
        public SerializableMethod(string containerAssemblyQualifiedName, string methodName) {
            container = new SerializableType(containerAssemblyQualifiedName);
            name = string.IsNullOrEmpty(methodName) ? null : methodName;
        }
        public static implicit operator SerializableMethod(MethodInfo method) => new SerializableMethod(method);
    }
    [Serializable]
    public struct SerializableField {
        public SerializableType container;
        public string name;
        [JsonIgnore]
        public bool IsCreated { get => container.IsCreated && !string.IsNullOrEmpty(name); }
        [JsonIgnore]
        public FieldInfo Value { get => container.Value?.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static); }
        [JsonIgnore]
        public string FullName { get => IsCreated ? $"{container.FullName}.{name}" : string.Empty; }
        public SerializableField(FieldInfo field) {
            container = new SerializableType(field?.DeclaringType);
            name = field?.Name;
        }
        public SerializableField(string containerAssemblyQualifiedName, string name) {
            container = new SerializableType(containerAssemblyQualifiedName);
            this.name = name;
        }
        public static implicit operator SerializableField(FieldInfo field) => new SerializableField(field);
        
    }


}