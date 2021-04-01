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
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var methods = GetMethods();

            if (methods == null)
                return null;

            var field = new PopupField<SerializableMethod>(property.displayName, methods, methods.First(), (method) => $"{method.container.FullName}/{method.name}", (method) => $"{method.container.FullName}/{method.name}");
            field.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("container.assemblyQualifiedName").stringValue = evt.newValue.container.AssemblyQualifiedName;
                property.FindPropertyRelative("name").stringValue = evt.newValue.name;
            });
            return field;
            
        }
        private List<SerializableMethod> GetMethods() {
            var providerType = fieldInfo.GetCustomAttribute<ProviderAttribute>()?.value;
            if (providerType != null && typeof(IMethodProvider).IsAssignableFrom(providerType)) {
                var provider = (IMethodProvider)Activator.CreateInstance(providerType);
                return provider.GetMethods();
            }
            else {
                return null;
            }
        }


    }
    public interface IMethodProvider {
        List<SerializableMethod> GetMethods();
    }
}