using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public class TypeFieldSchema : ScriptableObject, IInit {
        public const string TypeFieldClassName = "type-field";
        [InitializeOnLoadMethod]
        private static void UpdateSchema() {
            CompilationPipeline.assemblyCompilationFinished -= UpdateSchema;
            CompilationPipeline.assemblyCompilationFinished += UpdateSchema;
        }
        private static void UpdateSchema(string assemblyPath, CompilerMessage[] messages) => UpdateSchema(ProjectUtility.GetSingleton<TypeFieldSchema>(), assemblyPath, true, true);
        private static void UpdateSchema(TypeFieldSchema data, string assemblyPath, bool cleanElements, bool import) {

            if (data == null)
                return;
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            if (cleanElements) {
                data.entries.RemoveAll(e => !e.provider.IsCreated || e.provider.Value.Assembly.FullName == assembly.FullName);
                data.rules.RemoveAll(e => !e.provider.IsCreated || e.provider.Value.Assembly.FullName == assembly.FullName);
            }

            bool dirty = false;
            bool sortRules = false;
            foreach (var module in assembly.Modules) {

                foreach (var type in module.Types) {
                    if (type.HasCustomAttributes) {
                        foreach (var attr in type.CustomAttributes) {
                            if (attr.AttributeType.FullName == typeof(TypeFieldProviderAttribute).FullName) {
                                var targetType = ((TypeReference)attr.ConstructorArguments[0].Value).Resolve(); ;
                                var targetQualifiedName = targetType.FullName + ", " + targetType.Module.Assembly.FullName;
                                var providerQualifiedName = type.FullName + ", " + type.Module.Assembly.FullName;
                                dirty = true;
                                data.entries.Add(new Element
                                {
                                    target = new SerializableType(targetQualifiedName),
                                    provider = new SerializableType(providerQualifiedName)
                                });
                            }
                            else if (attr.AttributeType.FullName == typeof(TypeFieldProviderRuleAttribute).FullName) {
                                sortRules = true;
                                dirty = true;
                                var providerQualifiedName = type.FullName + ", " + type.Module.Assembly.FullName;
                                var priority = attr.HasConstructorArguments ? (int)attr.ConstructorArguments[0].Value : 0;
                                data.rules.Add(new Rule
                                {
                                    provider = new SerializableType(providerQualifiedName),
                                    priority = priority
                                });
                            }
                        }
                    }

                }
            }
            if (dirty && import) {
                if (sortRules) {
                    data.rules.Sort();
                }
                EditorUtility.SetDirty(data);
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data));
            }
        }
        [HideInInspector]
        public List<Rule> rules = new List<Rule>();
        [NonSerialized]
        private List<TypeFieldProviderRule> cachedRules = null;
        [HideInInspector]
        public List<Element> entries = new List<Element>();


        public bool CreateField(Type type, FieldInfo fieldInfo, object initialValue, ITypeFieldProviderContext context, out BindableElement element) {
            if (cachedRules == null) {
                cachedRules = rules.ConvertAll((r) => (TypeFieldProviderRule)Activator.CreateInstance(r.provider));
            }
            element = null;
            foreach (var rule in cachedRules) {
                if (rule.CreateField(this, type, fieldInfo, initialValue, out element)) {
                    context.HandleField(type, fieldInfo, element);
                    return true;
                }
            }
            return false;

        }
        public bool CreateField(Type type, FieldInfo fieldInfo, object initialValue, out BindableElement element) {
            if (cachedRules == null) {
                cachedRules = rules.ConvertAll((r) => (TypeFieldProviderRule)Activator.CreateInstance(r.provider));
            }
            element = null;
            foreach (var rule in cachedRules) {
                if (rule.CreateField(this, type, fieldInfo, initialValue, out element)) {
                    element.AddToClassList(TypeFieldClassName);
                    return true;
                }
            }
            return false;

        }
        public TypeFieldProvider GetProvider(Type type) {
            var p = entries.Find(e => e.target == type).provider.Value;
            if (p == null || !typeof(TypeFieldProvider).IsAssignableFrom(p))
                return null;
            return (TypeFieldProvider)Activator.CreateInstance(p);
        }

        public void Init() {
            foreach (var assemblyPath in Directory.GetFiles("Library/ScriptAssemblies").Where(f => f.EndsWith("dll"))) {
                UpdateSchema(this, assemblyPath, false, false);
            }
        }
        [Serializable]
        public struct Rule : IComparable<Rule> {
            public SerializableType provider;
            public int priority;

            public int CompareTo(Rule other) {
                return other.priority.CompareTo(priority);
            }
        }
        [Serializable]
        public struct Element {
            public SerializableType target;
            public SerializableType provider;
        }

    }
}