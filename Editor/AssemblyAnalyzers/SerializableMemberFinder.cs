using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NeroWeNeed.Commons.AssemblyAnalyzers.Editor;
using NeroWeNeed.Commons.Editor;
using UnityEngine;

[assembly: Analyzer(typeof(SerializableMemberFinder))]
namespace NeroWeNeed.Commons.Editor {
    public class SerializableMemberFinder : ITypeAnalyzer, IBeginAnalysis, IEndAnalysis {
        private List<TypeAnalysisData> filterData = new List<TypeAnalysisData>();
        public SerializableMemberCache cache;
        private bool updateCacheForAssembly = false;
        public string assembly;
        public void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition) {
            foreach (var fd in filterData) {
                if (fd.filter.IsValid(typeDefinition)) {
                    fd.filteredTypes.Add(new SerializableType(typeDefinition.AssemblyQualifiedName()));
                    updateCacheForAssembly = true;
                }
            }
        }
        public bool IsValid(TypeDefinition definition) {
            return !filterData.IsEmpty();
        }
        public void OnBeginAnalysis(AssemblyDefinition assemblyDefinition) {
            this.assembly = assemblyDefinition.FullName;
            if (cache == null) {
                cache = ProjectUtility.GetProjectAsset<SerializableMemberCache>();
            }

            if (cache != null) {

                filterData = cache.typeFields.Select(field => (field: field.IsCreated ? field.Value : null, filter: new TypeFilter(field.Value?.GetCustomAttributes<TypeFilterAttribute>()?.ToArray())))
                .Where(obj => obj.field != null && obj.field.GetCustomAttribute<HideInInspector>() == null && obj.filter.filterAttributes.Length > 0)
                .Select(obj => new TypeAnalysisData(obj.field, obj.filter)).ToList();
            }
            updateCacheForAssembly = false;
        }
        public void OnEndAnalysis(AssemblyDefinition assemblyDefinition) {
            if (cache == null) {
                cache = ProjectUtility.GetProjectAsset<SerializableMemberCache>();
            }
            if (cache != null) {
                if (updateCacheForAssembly) {
                    cache.assemblyData[assembly] = new SerializableMemberCache.AssemblyData { types = filterData.Where(fd => !fd.filteredTypes.IsEmpty()).ToDictionary(d => d.FullName, d => d.filteredTypes) };
                }
                else {
                    cache.assemblyData.Remove(assembly);
                }

                ProjectUtility.UpdateProjectAsset(cache);
            }
        }
        private class TypeAnalysisData {
            public FieldInfo field;
            public TypeFilter filter;
            public List<SerializableType> filteredTypes = new List<SerializableType>();
            public string FullName { get => field != null ? $"{field.DeclaringType.FullName}.{field.Name}" : string.Empty; }

            public TypeAnalysisData(FieldInfo field, TypeFilter filter) {
                this.field = field;
                this.filter = filter;
            }
        }
    }
}
