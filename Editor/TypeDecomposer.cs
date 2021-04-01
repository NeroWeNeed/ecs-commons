using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public static class TypeDecomposer {
        private static string JoinPath(string basePath, string next, char separator = '.') => string.IsNullOrEmpty(basePath) ? next : $"{basePath}{separator}{next}";
        public static void Decompose(this Type type, Func<Type, FieldInfo, FieldInfo, string, string, TypeDecompositionOptions, bool> onDecomposition, TypeDecompositionOptions options = default) {
            if (!type.IsValueType) {
                throw new ArgumentException("Type must be Value Type.", nameof(type));
            }
            if (onDecomposition == null) {
                throw new ArgumentException("No onDecomposition Callback detected.", nameof(onDecomposition));
            }
            var schema = ProjectUtility.GetOrCreateProjectAsset<TypeFieldSchema>();
            foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(field => field.GetCustomAttribute<HideInInspector>() == null)) {
                Decompose(field, null, JoinPath(options.initialPath, field.Name), string.Empty, onDecomposition, options);
            }
        }
        private static void Decompose(FieldInfo fieldInfo, FieldInfo parentFieldInfo, string path, string parentPath, Func<Type, FieldInfo, FieldInfo, string, string, TypeDecompositionOptions, bool> onDecomposition, TypeDecompositionOptions options) {
            if (!onDecomposition.Invoke(fieldInfo.FieldType, fieldInfo, parentFieldInfo, path, parentPath, options) && options.exploreChildren && Type.GetTypeCode(fieldInfo.FieldType) == TypeCode.Object && fieldInfo.FieldType.IsValueType) {
                foreach (var field in fieldInfo.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(field => field.GetCustomAttribute<HideInInspector>() == null)) {
                    Decompose(field, fieldInfo, JoinPath(path, field.Name), path, onDecomposition, options);
                }
            }

        }
    }
    public struct TypeDecompositionOptions {
        public bool exploreChildren;
        public string initialPath;
    }
}