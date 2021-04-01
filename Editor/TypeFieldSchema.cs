using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    

    [ProjectAsset(ProjectAssetType.Json)]
    [Serializable]
    public class TypeFieldSchema : IInitializable, IDeserializationCallback {
        public const string TypeFieldClassName = "type-field";
        [InitializeOnLoadMethod]
        private static void RegisterSchemaUpdateCallbacks() {
            CompilationPipeline.assemblyCompilationFinished -= UpdateSchema;
            CompilationPipeline.assemblyCompilationFinished += UpdateSchema;
        }
        private static void UpdateSchema(string assemblyPath, CompilerMessage[] messages) {
            var asset = ProjectUtility.GetProjectAsset<TypeFieldSchema>();
            if (asset != null) {
                asset.UpdateSchema(assemblyPath);
                ProjectUtility.UpdateProjectAsset(asset);
            }
        }
        public Dictionary<string, AssemblyData> assemblies = new Dictionary<string, AssemblyData>();
        [JsonIgnore]
        public ReadOnlyCollection<Rule> Rules { get; private set; }
        [JsonIgnore]
        public ReadOnlyDictionary<Type, Provider> Providers { get; private set; }

        internal void UpdateSchema(string assemblyPath, bool updateRules = true) {
            using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            
            var data = new AssemblyData();
            foreach (var module in assembly.Modules) {
                foreach (var type in module.Types) {
                    if (type.HasCustomAttributes) {
                        foreach (var attr in type.CustomAttributes) {
                            if (attr.AttributeType.FullName == typeof(TypeFieldProviderAttribute).FullName) {
                                var targetTypeReference = ((TypeReference)attr.ConstructorArguments[0].Value).Resolve();
                                var targetQualifiedName = targetTypeReference.FullName + ", " + targetTypeReference.Module.Assembly.FullName;
                                var providerQualifiedName = type.FullName + ", " + type.Module.Assembly.FullName;
                                var targetType = Type.GetType(targetQualifiedName);
                                data.providers[targetType] = new Provider { provider = Type.GetType(providerQualifiedName) };
                            }
                            else if (attr.AttributeType.FullName == typeof(TypeFieldProviderRuleAttribute).FullName) {
                                var providerQualifiedName = type.FullName + ", " + type.Module.Assembly.FullName;
                                var priority = attr.HasConstructorArguments ? (int)attr.ConstructorArguments[0].Value : 0;
                                data.rules.Add(new Rule
                                {
                                    provider = Type.GetType(providerQualifiedName),
                                    priority = priority
                                });
                            }
                        }
                    }
                }
            }
            if (data.IsEmpty) {
                this.assemblies.Remove(assembly.FullName);
            }
            else {
                this.assemblies[assembly.FullName] = data;
            }

            if (updateRules) {
                UpdateView();
            }
        }
        public void OnInit() {
            foreach (var assemblyPath in Directory.GetFiles("Library/ScriptAssemblies").Where(f => f.EndsWith("dll"))) {
                UpdateSchema(assemblyPath, false);
            }
            UpdateView();
        }

        private void UpdateView() {
            var rules = assemblies.SelectMany(assembly => assembly.Value.rules).ToList();
            rules.Sort();
            this.Rules = rules.AsReadOnly();
            this.Providers = new ReadOnlyDictionary<Type, Provider>(assemblies.SelectMany(assembly => assembly.Value.providers).ToDictionary(kv => kv.Key, kv => kv.Value));
        }
        public void OnDeserialize() {
            UpdateView();
        }
        public bool CreateField(Type type, FieldInfo fieldInfo, object initialValue, out BindableElement element) {
            element = null;
            foreach (var rule in Rules) {
                if (rule.Value.CreateField(this, type, fieldInfo, initialValue, out element)) {
                    element.AddToClassList(TypeFieldClassName);
                    return true;
                }
            }
            return false;

        }
        public TypeFieldProvider GetProvider(Type type) {
            if (Providers.TryGetValue(type, out Provider provider)) {
                return provider.Value;
            }
            else {
                return null;
            }
        }
        [Serializable]
        public class AssemblyData {
            public Dictionary<Type, Provider> providers = new Dictionary<Type, Provider>();
            public List<Rule> rules = new List<Rule>();
            [JsonIgnore]
            public bool IsEmpty { get => providers.Count == 0 && rules.Count == 0; }
        }
        [Serializable]
        public class Rule : IComparable<Rule> {
            public Type provider;
            public int priority;
            [JsonIgnore]
            private TypeFieldProviderRule value = null;
            [JsonIgnore]
            public TypeFieldProviderRule Value
            {
                get
                {
                    if (value == null) {
                        value = (TypeFieldProviderRule)Activator.CreateInstance(provider);
                    }
                    return value;
                }
            }

            public int CompareTo(Rule other) {
                return other.priority.CompareTo(priority);
            }
        }
        [Serializable]
        public class Provider {
            public Type provider;
            [JsonIgnore]
            private TypeFieldProvider value;
            [JsonIgnore]
            public TypeFieldProvider Value
            {
                get
                {
                    if (value == null) {
                        value = (TypeFieldProvider)Activator.CreateInstance(provider);
                    }
                    return value;
                }
            }
        }

    }
}