using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class TypeExtensions {
        public static FieldInfo[] GetSerializableFields(this Type type) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(t => (t.IsPublic && t.GetCustomAttribute<NonSerializedAttribute>() == null) || (!t.IsPublic && t.GetCustomAttribute<SerializeField>() != null)).ToArray();
        public static FieldInfo[] GetSerializableFields(this Type type, Func<FieldInfo, bool> predicate) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(fieldInfo => ((fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null) || (!fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<SerializeField>() != null)) && predicate.Invoke(fieldInfo)).ToArray();
        public static bool IsSerializable(this FieldInfo field) => (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() == null) || (!field.IsPublic && field.GetCustomAttribute<SerializeField>() != null);
        public static object GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        public static MethodInfo GetGenericMethod(this Type type, string name, BindingFlags flags = BindingFlags.Default) => type.GetMethods(flags).FirstOrDefault(m => m.Name == name && m.IsGenericMethod);
        public static string[] GetPropertyPaths(this Type type, Func<FieldInfo, bool> predicate = null) => (predicate == null ? type.GetSerializableFields().Select(field => field.Name) : type.GetSerializableFields().Where(predicate).Select(field => field.Name)).ToArray();
    }


}