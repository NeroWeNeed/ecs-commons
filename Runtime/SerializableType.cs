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
        public string AssemblyQualifiedName { get => assemblyQualifiedName; }
        public string FullName { get => assemblyQualifiedName.Substring(0,assemblyQualifiedName.IndexOf(',')); }
        [NonSerialized]
        private Type value;
        public Type Value
        {
            get
            {
                if (value == null && IsCreated) {
                    value = Type.GetType(assemblyQualifiedName);
                }
                return value;
            }
        }
        public bool IsCreated { get => !string.IsNullOrEmpty(assemblyQualifiedName); }
        public SerializableType(Type type) {
            assemblyQualifiedName = type?.AssemblyQualifiedName;
            this.value = type;
        }
        public SerializableType(string assemblyQualifiedName) {
            this.assemblyQualifiedName = assemblyQualifiedName;
            this.value = null;

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