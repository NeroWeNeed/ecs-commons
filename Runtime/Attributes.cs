using System;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.Commons {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class ColorAttribute : Attribute {
        public Color Value { get; }
        public Type Type { get; }

        public ColorAttribute(string value) : this(null, value) { }
        public ColorAttribute(byte red, byte green, byte blue, byte alpha = 255) : this(null, red, green, blue, alpha) { }
        public ColorAttribute(float red, float green, float blue, float alpha = 1f) : this(null, red, green, blue, alpha) { }
        public ColorAttribute(Type type, string value) {
            if (ColorUtility.TryParseHtmlString(value, out Color result)) {
                Value = result;
            }
            else {
                Value = Color.black;
            }
            Type = type;
        }
        public ColorAttribute(Type type, byte red, byte green, byte blue, byte alpha = 255) : this(red / 255f, green / 255f, blue / 255f, alpha / 255f) { }
        public ColorAttribute(Type type, float red, float green, float blue, float alpha = 1f) {
            Type = type;
            Value = new Color(math.clamp(red, 0, 1), math.clamp(green, 0, 1), math.clamp(blue, 0, 1), math.clamp(alpha, 0, 1));
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ProjectAssetAttribute : Attribute {
        public ProjectAssetType type;

        public string extension;

        public ProjectAssetAttribute(ProjectAssetType type, string extension) {
            this.type = type;
            this.extension = extension;
        }
        public ProjectAssetAttribute(ProjectAssetType type) {
            this.type = type;
            switch (type) {
                case ProjectAssetType.Bytes:
                    this.extension = "bytes";
                    break;
                case ProjectAssetType.Xml:
                    this.extension = "xml";
                    break;
                case ProjectAssetType.Json:
                    this.extension = "json";
                    break;
            }

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ProviderAttribute : Attribute {
        public Type value;

        public ProviderAttribute(Type value) {
            this.value = value;
        }
    }
    public enum ProjectAssetType : byte {
        Bytes = 0,
        Xml = 1,
        Json = 2
    }
}