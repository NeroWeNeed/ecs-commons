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
        private static readonly Dictionary<TypeFilter, List<SerializableType>> types = new Dictionary<TypeFilter, List<SerializableType>>();
        private int index = -1;
        private const string NO_TYPE = "None";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var qualifiedName = property.FindPropertyRelative("assemblyQualifiedName");
            var types = GetTypes(property);
            types.Insert(0, null);
            var type = string.IsNullOrEmpty(qualifiedName.stringValue) ? null : Type.GetType(qualifiedName.stringValue);
            index = types.IndexOf(type);
            int newIndex = EditorGUI.Popup(position, label, index < 0 ? 0 : index, types.Select(t => new GUIContent(t.Value?.FullName ?? NO_TYPE)).ToArray());
            if (newIndex != index && newIndex >= 0) {
                qualifiedName.stringValue = types[newIndex].Value?.AssemblyQualifiedName;
                index = newIndex;
            }
        }
        private TypeFilter GetTypeFilter() {
            if (fieldInfo == null)
                return new TypeFilter(Array.Empty<TypeFilterAttribute>());
            return new TypeFilter(fieldInfo.GetCustomAttributes(typeof(TypeFilterAttribute), true) as TypeFilterAttribute[]);
        }
        private List<SerializableType> GetTypes(SerializedProperty property) {
            var filter = GetTypeFilter();
            if (!SerializableTypeDrawer.types.TryGetValue(filter, out List<SerializableType> filteredTypes)) {
                filteredTypes = filter.CollectTypesAsSerializable();
                SerializableTypeDrawer.types[filter] = filteredTypes;
            }
            return filteredTypes;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");
            var types = GetTypes(property).ConvertAll(type => type.Value?.AssemblyQualifiedName);
            types.Insert(0, string.Empty);
            var popup = new PopupField<string>(property.displayName, types, string.IsNullOrWhiteSpace(assemblyQualifiedNameProperty.stringValue) ? string.Empty : assemblyQualifiedNameProperty.stringValue, t => string.IsNullOrWhiteSpace(t) ? NO_TYPE : t, t => string.IsNullOrWhiteSpace(t) ? NO_TYPE : t);
            popup.BindProperty(assemblyQualifiedNameProperty);
            return popup;
        }

    }
}