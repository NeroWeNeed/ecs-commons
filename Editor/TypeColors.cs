using System;
using System.Collections.Generic;
using System.Reflection;
using NeroWeNeed.Commons;
using NUnit.Compatibility;
using UnityEditor;
using UnityEngine;

[assembly:Color(typeof(long),"#D91A27")]
[assembly: Color(typeof(int), "#A6141E")]
[assembly: Color(typeof(short), "#730E15")]
[assembly: Color(typeof(sbyte), "#40080B")]
[assembly: Color(typeof(ulong), "#1AD947")]
[assembly: Color(typeof(uint), "#14A636")]
[assembly: Color(typeof(ushort), "#0E7325")]
[assembly: Color(typeof(byte), "#084015")]
[assembly: Color(typeof(float), "#181E73")]
[assembly: Color(typeof(double), "#232CA6")]
[assembly: Color(typeof(decimal), "#2E39D9")]
[assembly: Color(typeof(string), "#D9B123")]
[assembly: Color(typeof(char), "#735E12")]
[assembly: Color(typeof(bool), "#6B1375")]

namespace NeroWeNeed.Commons.Editor {
    public static class TypeColors {
        private static readonly Dictionary<Type, Color> Colors = new Dictionary<Type, Color>();

        [InitializeOnLoadMethod]
        private static void GetAssemblyColors() {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var attrs = assembly.GetCustomAttributes<ColorAttribute>();
                foreach (var attr in attrs) {
                    if (attr.Type != null) {
                        Colors[attr.Type] = attr.Value;
                    }
                }
            }
        }
        public static Color GetColor(this Type type, Color defaultColor) {
            if (Colors.TryGetValue(type, out Color color)) {
                return color;
            }
            else {
                var attr = type.GetCustomAttribute<ColorAttribute>();
                if (attr == null) {
                    return defaultColor;
                }
                else {
                    Colors[type] = attr.Value;
                    return attr.Value;
                }
            }
        }
        public static Color GetColor(this Type type) => GetColor(type, Color.black);
        public static Color GetColor(this FieldInfo fieldInfo, Color defaultColor) {
            var attr = fieldInfo.GetCustomAttribute<ColorAttribute>();
            if (attr == null) {
                return GetColor(fieldInfo.FieldType, defaultColor);
            }
            else {
                return attr.Value;
            }
        }
        public static Color GetColor(this FieldInfo fieldInfo) => GetColor(fieldInfo, Color.black);

/*         private static Color GetColor(ColorAttribute attribute, object source, bool ignoreAlpha) {
            if (attribute != null) {
                Color color = attribute.Value;
                if (ignoreAlpha) {
                    color.a = 1f;
                }
                return color;
            }
            else if (hardCodedColors.TryGetValue(source, out Color color)) {
                if (ignoreAlpha) {
                    color.a = 1f;
                }
                return color;
            }
            else {
                var hash = source.GetHashCode();
                var a = ignoreAlpha ? 1f : (0b00000000000000000000000011111111 & hash) / 255f;
                var b = ((0b00000000000000001111111100000000 & hash) >> 8) / 255f;
                var g = ((0b00000000111111110000000000000000 & hash) >> 16) / 255f;
                var r = ((0b11111111000000000000000000000000 & hash) >> 24) / 255f;
                return new Color(r, g, b, a);
            }
        } */
    }
}