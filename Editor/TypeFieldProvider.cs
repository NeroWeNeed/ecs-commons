using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public abstract class TypeFieldProvider {
        public abstract BindableElement CreateField(Type type, FieldInfo fieldInfo, object initialValue);
    }
    public abstract class TypeFieldProviderRule {
        public abstract bool CreateField(TypeFieldSchema schema,Type type, FieldInfo fieldInfo, object initialValue,out BindableElement element);
    }
    public interface ITypeFieldProviderContext {
        void HandleField(Type type, FieldInfo fieldInfo, BindableElement element);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TypeFieldProviderAttribute : Attribute {
        public Type value;

        public TypeFieldProviderAttribute(Type value) {
            this.value = value;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TypeFieldProviderRuleAttribute : Attribute {
        public int priority;

        public TypeFieldProviderRuleAttribute(int priority = 0) {
            this.priority = priority;
        }
    }
}