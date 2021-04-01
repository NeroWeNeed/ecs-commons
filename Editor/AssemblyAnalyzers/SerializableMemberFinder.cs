using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NeroWeNeed.Commons.AssemblyAnalyzers.Editor;
using NeroWeNeed.Commons.Editor;
using UnityEngine;

[assembly:Analyzer(typeof(SerializableMemberFinder))]
namespace NeroWeNeed.Commons.Editor {
    public class SerializableMemberFinder : ITypeAnalyzer, IBeginAnalysis, IEndAnalysis {
        private Dictionary<string, TypeFilter> filters = null;
        private Dictionary<string, List<SerializableType>> types = new Dictionary<string, List<SerializableType>>();
        public SerializableMemberCache cache;
        public string assembly;
        public void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition) {
            foreach (var filter in filters.ValueNotNull()) {
                if (filter.Value.IsValid(typeDefinition)) {
                    if (!types.TryGetValue(filter.Key, out var filteredTypes)) {
                        filteredTypes = new List<SerializableType>();
                        types[filter.Key] = filteredTypes;
                    }
                    filteredTypes.Add(new SerializableType(typeDefinition.AssemblyQualifiedName()));
                }
            }
        }
        public bool IsValid(TypeDefinition definition) {
            return filters != null;
        }
        public void OnBeginAnalysis(AssemblyDefinition assemblyDefinition) {
            this.assembly = assemblyDefinition.FullName;
            if (cache == null) {
                cache = ProjectUtility.GetProjectAsset<SerializableMemberCache>();
            }
            
            if (cache != null) {
                filters = cache.typeFields.Select(name =>
                {
                    var separator = name.LastIndexOf('.');
                    var typeName = name.Substring(0, separator);
                    var fieldName = name.Substring(separator + 1, name.Length - (typeName.Length + 1));
                    var type = Type.GetType(typeName);
                    var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    return (name, field, attrs: field?.GetCustomAttributes<TypeFilterAttribute>()?.ToArray());
                }).Where(a => a.field != null  && a.field.GetCustomAttribute<HideInInspector>() == null && a.attrs?.IsEmpty() == false).ToDictionary(a => a.name, a => new TypeFilter(a.attrs));
            }
        }
        public void OnEndAnalysis(AssemblyDefinition assemblyDefinition) {
            if (cache == null) {
                cache = ProjectUtility.GetProjectAsset<SerializableMemberCache>();
            }
            if (cache != null) {
                if (types.IsEmpty()) {
                    cache.assemblyData.Remove(assembly);
                }
                else {
                    cache.assemblyData[assembly] = new SerializableMemberCache.AssemblyData { types = types };
                }
                ProjectUtility.UpdateProjectAsset(cache);
            }
        }
    }
}
