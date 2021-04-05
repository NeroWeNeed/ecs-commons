using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Compatibility;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace NeroWeNeed.Commons.AssemblyAnalyzers.Editor {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AdditionalAssemblyAnalysisPath : Attribute {
        public string path;

        public AdditionalAssemblyAnalysisPath(string path) {
            this.path = path;
        }
    }

    public interface IValidator<TDefinition> {
        bool IsValid(TDefinition definition);
    }
    public interface IMemberAnalyzer {
        bool IsExplorable(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition);
    }
    public interface IAssemblyAnalyzer : IValidator<AssemblyDefinition> {
        void Analyze(AssemblyDefinition assemblyDefinition);
    }
    public interface IModuleAnalyzer : IValidator<ModuleDefinition> {
        void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition);
    }
    public interface ITypeAnalyzer : IValidator<TypeDefinition> {
        void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition);
    }
    public interface IMethodAnalyzer : IValidator<MethodDefinition>, IMemberAnalyzer {
        void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition, MethodDefinition methodDefinition);
    }
    public interface IFieldAnalyzer : IValidator<FieldDefinition>, IMemberAnalyzer {
        void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition, FieldDefinition fieldDefinition);
    }
    public interface IPropertyAnalyzer : IValidator<PropertyDefinition>, IMemberAnalyzer {
        void Analyze(AssemblyDefinition assemblyDefinition, ModuleDefinition moduleDefinition, TypeDefinition typeDefinition, PropertyDefinition fieldDefinition);
    }
    public interface IBeginAnalysis {
        void OnBeginAnalysis(AssemblyDefinition assemblyDefinition);
    }
    public interface IEndAnalysis {
        void OnEndAnalysis(AssemblyDefinition assemblyDefinition);
    }
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AnalyzerAttribute : Attribute {
        public Type value;

        public AnalyzerAttribute(Type analyzer) {
            this.value = analyzer;
        }
    }

    [InitializeOnLoad]
    public static class AssemblyAnalyzer {
        private static readonly List<IAssemblyAnalyzer> assemblyAnalyzers = new List<IAssemblyAnalyzer>();
        private static readonly List<ITypeAnalyzer> typeAnalyzers = new List<ITypeAnalyzer>();
        private static readonly List<IMethodAnalyzer> methodAnalyzers = new List<IMethodAnalyzer>();
        private static readonly List<IFieldAnalyzer> fieldAnalyzers = new List<IFieldAnalyzer>();
        private static readonly List<IPropertyAnalyzer> propertyAnalyzers = new List<IPropertyAnalyzer>();
        private static readonly List<IModuleAnalyzer> moduleAnalyzers = new List<IModuleAnalyzer>();
        private static readonly List<object> analyzers = new List<object>();
        private static readonly List<string> additionalAssemblyPaths = new List<string>();
        static AssemblyAnalyzer() {
            InitializeAnalyzers();
            CompilationPipeline.assemblyCompilationFinished -= AnalyzeAssembly;
            CompilationPipeline.assemblyCompilationFinished += AnalyzeAssembly;
        }
        private static void InitializeAnalyzers() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var analyzerType in assembly.GetCustomAttributes<AnalyzerAttribute>().Select(attr => attr?.value).NotNull().Where(type => !type.IsGenericType)) {
                    try {
                        var analyzer = Activator.CreateInstance(analyzerType);
                        analyzers.Add(analyzer);
                        if (analyzer is IAssemblyAnalyzer assemblyAnalyzer) {
                            assemblyAnalyzers.Add(assemblyAnalyzer);
                        }
                        if (analyzer is ITypeAnalyzer typeAnalyzer) {
                            typeAnalyzers.Add(typeAnalyzer);
                        }
                        if (analyzer is IMethodAnalyzer methodAnalyzer) {
                            methodAnalyzers.Add(methodAnalyzer);
                        }
                        if (analyzer is IFieldAnalyzer fieldAnalyzer) {
                            fieldAnalyzers.Add(fieldAnalyzer);
                        }
                        if (analyzer is IPropertyAnalyzer propertyAnalyzer) {
                            propertyAnalyzers.Add(propertyAnalyzer);
                        }
                        if (analyzer is IModuleAnalyzer moduleAnalyzer) {
                            moduleAnalyzers.Add(moduleAnalyzer);
                        }
                    }
                    catch (Exception) {
                        continue;
                    }
                }
                foreach (var alsoAnalyze in assembly.GetCustomAttributes<AdditionalAssemblyAnalysisPath>().Select(attr => attr?.path).NotNullOrWhiteSpace().Where(path => path.EndsWith(".dll"))) {
                    additionalAssemblyPaths.Add(alsoAnalyze);
                }
            }
        }
        private static void AnalyzeAssembly(string path, CompilerMessage[] messages) {
            AnalyzeAssembly(path);
        }
        private static bool ShouldExploreModules() => moduleAnalyzers.Count > 0 || ShouldExploreTypes();
        private static bool ShouldExploreTypes() => typeAnalyzers.Count > 0 || ShouldExploreMembers();
        private static bool ShouldExploreMembers() => fieldAnalyzers.Count > 0 || methodAnalyzers.Count > 0 || propertyAnalyzers.Count > 0;
        private static bool ShouldExploreModules(object analyzer) => analyzer is IModuleAnalyzer || ShouldExploreTypes(analyzer);
        private static bool ShouldExploreTypes(object analyzer) => analyzer is ITypeAnalyzer || ShouldExploreMembers(analyzer);
        private static bool ShouldExploreMembers(object analyzer) => analyzer is IFieldAnalyzer || analyzer is IMethodAnalyzer || analyzer is IPropertyAnalyzer;

        public static void AnalyzeAssemblies(object analyzer) {

            foreach (var assemblyPath in Directory.GetFiles("Library/ScriptAssemblies").Where(f => f.EndsWith(".dll"))) {
                try {
                    AnalyzeAssembly(analyzer, assemblyPath);
                }
                catch (Exception) { }
            }
            foreach (var assemblyPath in additionalAssemblyPaths) {
                try {
                    AnalyzeAssembly(analyzer, assemblyPath);
                }
                catch (Exception) { }
            }
        }
        public static void AnalyzeAssemblies() {
            foreach (var assemblyPath in Directory.GetFiles("Library/ScriptAssemblies").Where(f => f.EndsWith(".dll"))) {
                try {
                    AnalyzeAssembly(assemblyPath);
                }
                catch (Exception) { }
            }
            foreach (var assemblyPath in additionalAssemblyPaths) {
                try {
                    AnalyzeAssembly(assemblyPath);
                }
                catch (Exception) { }
            }
        }

        public static void AnalyzeAssembly(string path) {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory("Library/ScriptAssemblies");
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
            foreach (var analyzer in analyzers.OfType<IBeginAnalysis>()) {
                analyzer.OnBeginAnalysis(assemblyDefinition);
            }
            foreach (var assemblyAnalyzer in assemblyAnalyzers) {
                if (assemblyAnalyzer.IsValid(assemblyDefinition)) {
                    assemblyAnalyzer.Analyze(assemblyDefinition);
                }
            }
            var applicableMethodAnalyzers = new List<IMethodAnalyzer>();
            var applicableFieldAnalyzers = new List<IFieldAnalyzer>();
            var applicablePropertyAnalyzers = new List<IPropertyAnalyzer>();
            if (ShouldExploreModules()) {
                foreach (var moduleDefinition in assemblyDefinition.Modules) {
                    foreach (var moduleAnalyzer in moduleAnalyzers) {
                        if (moduleAnalyzer.IsValid(moduleDefinition)) {
                            moduleAnalyzer.Analyze(assemblyDefinition, moduleDefinition);
                        }
                    }
                    if (ShouldExploreTypes()) {
                        foreach (var typeDefinition in moduleDefinition.Types) {
                            foreach (var typeAnalyzer in typeAnalyzers) {
                                if (typeAnalyzer.IsValid(typeDefinition)) {
                                    typeAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition);
                                }
                            }
                            if (ShouldExploreMembers()) {

                                if (typeDefinition.HasMethods)
                                    applicableMethodAnalyzers.AddRange(methodAnalyzers.Where(methodAnalyzer => methodAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)));
                                if (typeDefinition.HasFields)
                                    applicableFieldAnalyzers.AddRange(fieldAnalyzers.Where(fieldAnalyzer => fieldAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)));
                                if (typeDefinition.HasProperties)
                                    applicablePropertyAnalyzers.AddRange(propertyAnalyzers.Where(propertyAnalyzer => propertyAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)));
                                if (applicableFieldAnalyzers.Count > 0) {
                                    foreach (var fieldDefinition in typeDefinition.Fields) {
                                        foreach (var fieldAnalyzer in applicableFieldAnalyzers) {
                                            if (fieldAnalyzer.IsValid(fieldDefinition)) {
                                                fieldAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, fieldDefinition);
                                            }
                                        }
                                    }
                                }
                                if (applicablePropertyAnalyzers.Count > 0) {
                                    foreach (var propertyDefinition in typeDefinition.Properties) {
                                        foreach (var propertyAnalyzer in applicablePropertyAnalyzers) {
                                            if (propertyAnalyzer.IsValid(propertyDefinition)) {
                                                propertyAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, propertyDefinition);
                                            }
                                        }
                                    }
                                }
                                if (applicableMethodAnalyzers.Count > 0) {
                                    foreach (var methodDefiniton in typeDefinition.Methods) {
                                        foreach (var methodAnalyzer in applicableMethodAnalyzers) {
                                            if (methodAnalyzer.IsValid(methodDefiniton)) {
                                                methodAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, methodDefiniton);
                                            }
                                        }
                                    }
                                }

                            }
                            applicableFieldAnalyzers.Clear();
                            applicableMethodAnalyzers.Clear();
                            applicablePropertyAnalyzers.Clear();
                        }
                    }
                }
            }
            foreach (var analyzer in analyzers.OfType<IEndAnalysis>()) {
                analyzer.OnEndAnalysis(assemblyDefinition);
            }
        }
        public static void AnalyzeAssembly(object analyzer, string path) {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory("Library/ScriptAssemblies");
            using var assemblyDefinition = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
            if (analyzer is IBeginAnalysis begin) {
                begin.OnBeginAnalysis(assemblyDefinition);
            }
            if (analyzer is IAssemblyAnalyzer assemblyAnalyzer && assemblyAnalyzer.IsValid(assemblyDefinition)) {
                assemblyAnalyzer.Analyze(assemblyDefinition);
            }
            if (ShouldExploreModules(analyzer)) {
                foreach (var moduleDefinition in assemblyDefinition.Modules) {
                    if (analyzer is IModuleAnalyzer moduleAnalyzer && moduleAnalyzer.IsValid(moduleDefinition)) {
                        moduleAnalyzer.Analyze(assemblyDefinition, moduleDefinition);
                    }
                    if (ShouldExploreTypes(analyzer)) {
                        foreach (var typeDefinition in moduleDefinition.Types) {
                            if (analyzer is ITypeAnalyzer typeAnalyzer && typeAnalyzer.IsValid(typeDefinition)) {
                                typeAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition);
                            }
                            if (ShouldExploreMembers()) {
                                if (typeDefinition.HasFields && analyzer is IFieldAnalyzer fieldAnalyzer && fieldAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)) {
                                    foreach (var fieldDefinition in typeDefinition.Fields) {
                                        if (fieldAnalyzer.IsValid(fieldDefinition)) {
                                            fieldAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, fieldDefinition);
                                        }
                                    }
                                }
                                if (typeDefinition.HasProperties && analyzer is IPropertyAnalyzer propertyAnalyzer && propertyAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)) {
                                    foreach (var propertyDefinition in typeDefinition.Properties) {
                                        if (propertyAnalyzer.IsValid(propertyDefinition)) {
                                            propertyAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, propertyDefinition);
                                        }
                                    }
                                }
                                if (typeDefinition.HasMethods && analyzer is IMethodAnalyzer methodAnalyzer && methodAnalyzer.IsExplorable(assemblyDefinition, moduleDefinition, typeDefinition)) {
                                    foreach (var methodDefinition in typeDefinition.Methods) {
                                        if (methodAnalyzer.IsValid(methodDefinition)) {
                                            methodAnalyzer.Analyze(assemblyDefinition, moduleDefinition, typeDefinition, methodDefinition);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if (analyzer is IEndAnalysis end) {
                end.OnEndAnalysis(assemblyDefinition);
            }

        }

        public static Instruction BuildInstruction(this Instruction self, params Instruction[] instructions) {
            if (instructions.Length > 1) {
                var current = self;
                while (current.Next != null) {
                    current = current.Next;
                }
                for (int i = 1; i < instructions.Length; i++) {
                    current.Next = instructions[i];
                    current = current.Next;
                }
            }
            return self;
        }
    }
}