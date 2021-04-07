using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    [CustomPropertyDrawer(typeof(SerializableMethod))]
    public class SerializableMethodPropertyDrawer : PropertyDrawer {
        private const string NO_TYPE = "None";
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var methods = GetMethods(fieldInfo, property);
            if (methods == null)
                methods = new List<SerializableMethod>() { default };
            var current = new SerializableMethod(property.FindPropertyRelative("container.assemblyQualifiedName").stringValue, property.FindPropertyRelative("name").stringValue);
            
            var field = new PopupField<SerializableMethod>(property.displayName, methods, current, (method) => method.IsCreated ? $"{method.container.FullName}::{method.name}" : NO_TYPE, method => string.IsNullOrWhiteSpace(method.container.AssemblyQualifiedName) ? NO_TYPE : method.container.AssemblyQualifiedName.Substring(0, method.container.AssemblyQualifiedName.IndexOf(',')).Replace('.', '/') + $"::{method.name}");
            field.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("container.assemblyQualifiedName").stringValue = evt.newValue.container.AssemblyQualifiedName;
                property.FindPropertyRelative("name").stringValue = evt.newValue.name;
                property.serializedObject.ApplyModifiedProperties();
            });
            return field;
        }
        private List<SerializableMethod> GetMethods(FieldInfo fieldInfo, SerializedProperty property) {
            var providerType = fieldInfo.GetCustomAttribute<ProviderAttribute>()?.value;
            if (providerType != null && typeof(IMethodProvider).IsAssignableFrom(providerType)) {
                var provider = (IMethodProvider)Activator.CreateInstance(providerType);
                return provider.GetMethods(fieldInfo, property);
            }
            else {
                return new List<SerializableMethod>();
            }
        }
    }
    public interface IMethodProvider {
        List<SerializableMethod> GetMethods(FieldInfo fieldInfo, SerializedProperty property);
    }
}