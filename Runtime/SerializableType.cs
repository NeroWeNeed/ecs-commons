using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using UnityEngine;

namespace NeroWeNeed.Commons {
    [Serializable]
    [JsonConverter(typeof(SerializableTypeConverter))]
    public struct SerializableType : IEquatable<SerializableType>, IEquatable<Type> {

        [SerializeField]
        internal string assemblyQualifiedName;

        [SerializeField]
        internal string[] genericArguments;
        public string AssemblyQualifiedName { get => assemblyQualifiedName; }
        public bool IsGenericType { get => genericArguments?.Length > 0; }
        public string FullName { get => string.IsNullOrEmpty(assemblyQualifiedName) ? null : assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(',')); }
        [NonSerialized]
        private Type value;
        public Type Value
        {
            get
            {
                if (value == null && IsCreated) {
                    value = BuildType(assemblyQualifiedName, genericArguments);
                }
                return value;
            }
        }
        public bool IsCreated { get => !string.IsNullOrEmpty(assemblyQualifiedName); }
        public SerializableType(Type type) {
            assemblyQualifiedName = string.IsNullOrEmpty(type?.AssemblyQualifiedName) ? null : type.AssemblyQualifiedName;
            genericArguments = CollectGenericArguments(type);
            this.value = type;
        }
        public SerializableType(string assemblyQualifiedName, params string[] genericArguments) {
            this.assemblyQualifiedName = string.IsNullOrEmpty(assemblyQualifiedName) ? null : assemblyQualifiedName;
            this.genericArguments = genericArguments;
            this.value = null;

        }
        private static string[] CollectGenericArguments(Type type) {
            if (type.IsConstructedGenericType) {
                var types = new List<string>();
                CollectGenericArguments(type, types);
                return types.ToArray();
            }
            else {
                return null;
            }
        }
        private static void CollectGenericArguments(Type type, List<string> types) {
            types.Add(type.GetGenericTypeDefinition().AssemblyQualifiedName);
            if (type.IsConstructedGenericType) {
                foreach (var item in type.GenericTypeArguments) {
                    CollectGenericArguments(item, types);
                }
            }
        }
        private static Type BuildType(string baseName, string[] arguments) {
            Type current = Type.GetType(baseName);
            if (arguments?.IsEmpty() != false || current == null || !current.IsGenericType) {
                return current;
            }
            int index = 0;
            var types = new Type[current.GetGenericArguments().Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = BuildType(arguments, ref index);
            }
            return current.MakeGenericType(types);
        }
        private static Type BuildType(string[] arguments, ref int index) {
            var str = arguments[index++];
            Type current = Type.GetType(str);
            if (arguments?.IsEmpty() != false || current == null || !current.IsGenericType) {
                return current;
            }
            var types = new Type[current.GetGenericArguments().Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = BuildType(arguments, ref index);
            }
            return current.MakeGenericType(types);
        }

        public static implicit operator SerializableType(Type type) => new SerializableType(type);
        public static implicit operator Type(SerializableType type) => type.Value;

        public static bool operator ==(SerializableType self, Type type) {
            return self.Equals(type);
        }

        public static bool operator !=(SerializableType self, Type type) {
            return !self.Equals(type);
        }
        public static bool operator ==(SerializableType self, SerializableType type) {
            return self.Equals(type);
        }

        public static bool operator !=(SerializableType self, SerializableType type) {
            return !self.Equals(type);
        }

        public bool Equals(Type other) {
            return this.assemblyQualifiedName == other.AssemblyQualifiedName;
        }

        public bool Equals(SerializableType other) {
            return this.assemblyQualifiedName == other.assemblyQualifiedName;
        }

        public override bool Equals(object obj) {
            if (obj is SerializableType serializableType) {
                return Equals(serializableType);
            }
            else if (obj is Type type) {
                return Equals(type);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            int hashCode = 35819425;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(assemblyQualifiedName);
            return hashCode;
        }

        public override string ToString() {
            return assemblyQualifiedName;
        }
    }


}