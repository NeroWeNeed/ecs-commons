using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class TypeExtensions {
        public static FieldInfo[] GetSerializableFields(this Type type) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(t => (t.IsPublic && t.GetCustomAttribute<NonSerializedAttribute>() == null) || (!t.IsPublic && t.GetCustomAttribute<SerializeField>() != null)).ToArray();
        public static FieldInfo[] GetSerializableFields(this Type type, Predicate<Type> predicate) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(t => ((t.IsPublic && t.GetCustomAttribute<NonSerializedAttribute>() == null) || (!t.IsPublic && t.GetCustomAttribute<SerializeField>() != null)) && predicate.Invoke(t.FieldType)).ToArray();
        public static bool IsSerializable(this FieldInfo field) => (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() == null) || (!field.IsPublic && field.GetCustomAttribute<SerializeField>() != null);
        public static object GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

    }


}