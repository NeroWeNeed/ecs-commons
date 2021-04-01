using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using NeroWeNeed.Commons.AssemblyAnalyzers.Editor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class TypeFilterExtensions {
        public static bool IsValid(this TypeFilter filter, TypeDefinition definition) {
            return filter.IsValid(Type.GetType(definition.AssemblyQualifiedName()));
        }

    }
    public sealed class TypeFilter {
        public TypeFilterAttribute[] filterAttributes;

        public TypeFilter(TypeFilterAttribute[] filterAttributes) {
            this.filterAttributes = filterAttributes;
            Array.Sort(filterAttributes);
        }

        public override bool Equals(object obj) {
            return obj is TypeFilter filter && Array.Equals(filterAttributes, filter.filterAttributes);
        }

        public override int GetHashCode() {
            return -1995125846 + EqualityComparer<TypeFilterAttribute[]>.Default.GetHashCode(filterAttributes);
        }
        public bool IsValid(Type type) {
            foreach (var filterAttribute in filterAttributes) {
                if (!filterAttribute.IsValid(type))
                    return false;
            }
            return true;
        }

        public List<SerializableType> CollectSerializableTypes(FieldInfo fieldInfo) {

            var cache = ProjectUtility.GetOrCreateProjectAsset<SerializableMemberCache>();
            var path = fieldInfo.DeclaringType.AssemblyQualifiedName + '.' + fieldInfo.Name;
            if (!cache.types.TryGetValue(path, out List<SerializableType> result) && !cache.typeFields.Contains(path)) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    var output = new List<SerializableType>();
                    foreach (var type in assembly.GetTypes()) {
                        if (type.GetCustomAttribute<HideInInspector>() == null && IsValid(type)) {
                            output.Add(type);
                        }
                    }

                    if (!output.IsEmpty()) {
                        if (!cache.assemblyData.TryGetValue(assembly.FullName, out var data)) {
                            data = new SerializableMemberCache.AssemblyData();
                            cache.assemblyData[assembly.FullName] = data;
                        }
                        data.types[path] = output;
                    }
                }
                cache.typeFields.Add(path);
                cache.types.TryGetValue(path, out result);
                ProjectUtility.UpdateProjectAsset(cache);
                return result;
            }
            else {
                return result;
            }

        }

    }
}