using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {

    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : PropertyDrawer {
        private int index = -1;
        private const string NO_TYPE = "None";
        /*         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                    var qualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
                    var types = GetTypes() ?? new List<SerializableType>();

                    types.Insert(0, null);
                    var type = string.IsNullOrEmpty(qualifiedName.stringValue) ? null : Type.GetType(qualifiedName.stringValue);
                    index = types.IndexOf(type);
                    int newIndex = EditorGUI.Popup(position, label, index < 0 ? 0 : index, types.Select(t => new GUIContent(t.Value?.FullName ?? NO_TYPE)).ToArray());
                    if (newIndex != index && newIndex >= 0) {
                        qualifiedName.stringValue = types[newIndex].Value?.AssemblyQualifiedName;
                        index = newIndex;
                    }
                }
         */
        private List<SerializableType> GetTypes() {
            var filter = new TypeFilter(fieldInfo?.GetCustomAttributes<TypeFilterAttribute>(true)?.ToArray());
            return filter.CollectSerializableTypes(fieldInfo);
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");
            var assemblyQualifiedName = string.IsNullOrEmpty(assemblyQualifiedNameProperty.stringValue) ? string.Empty : assemblyQualifiedNameProperty.stringValue;
            var types = GetTypes();
            types.Insert(0, default);
            var initial = new SerializableType(Type.GetType(assemblyQualifiedName, false));
            if (!types.Contains(initial) || !initial.IsCreated) {
                assemblyQualifiedNameProperty.stringValue = string.Empty;
                property.serializedObject.ApplyModifiedProperties();
                initial = default;
            }
            var field = new PopupField<SerializableType>(property.displayName, types, initial, t => string.IsNullOrWhiteSpace(t.AssemblyQualifiedName) ? NO_TYPE : t.AssemblyQualifiedName.Substring(0, t.AssemblyQualifiedName.IndexOf(',')), t => string.IsNullOrWhiteSpace(t.AssemblyQualifiedName) ? NO_TYPE : t.AssemblyQualifiedName.Substring(0, t.AssemblyQualifiedName.IndexOf(',')).Replace('.', '/'));
            field.RegisterValueChangedCallback(evt =>
            {
                property.FindPropertyRelative("assemblyQualifiedName").stringValue = evt.newValue.AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            });

            return field;
        }
/*                 public override VisualElement CreatePropertyGUI(SerializedProperty property) {
                    var assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");
                    var assemblyQualifiedName = string.IsNullOrEmpty(assemblyQualifiedNameProperty.stringValue) ? string.Empty : assemblyQualifiedNameProperty.stringValue;
                    var types = GetTypes().ConvertAll(type => type.Value?.AssemblyQualifiedName);
                    types.Insert(0, string.Empty);
                    var initial = assemblyQualifiedName;
                    if (!types.Contains(assemblyQualifiedName)) {
                        //assemblyQualifiedNameProperty.stringValue = string.Empty;
                        initial = string.Empty;
                    }
                    return new PopupField<string>(property.displayName, types, initial, t => string.IsNullOrWhiteSpace(t) ? NO_TYPE : t.Substring(0, t.IndexOf(',')), t => string.IsNullOrWhiteSpace(t) ? NO_TYPE : t.Substring(0, t.IndexOf(',')).Replace('.', '/'))
                    {
                        bindingPath = assemblyQualifiedNameProperty.propertyPath
                    };
                } */

    }
}