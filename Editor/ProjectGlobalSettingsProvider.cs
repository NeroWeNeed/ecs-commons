using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public abstract class ProjectGlobalSettingsProvider : SettingsProvider {
        public abstract Type GlobalSettingsType { get; }


        public const string TemplateUxml = "Packages/github.neroweneed.commons/Editor/Resources/ProjectGlobalSettingsTemplate.uxml";
        public const string TemplateUss = "Packages/github.neroweneed.commons/Editor/Resources/ProjectGlobalSettingsTemplate.uss";
        public const string MissingSettingsUxml = "Packages/github.neroweneed.commons/Editor/Resources/MissingSettings.uxml";
        public string uxmlPath = null;
        public string ussPath = null;
        public string AssetPath { get => $"{ProjectUtility.SettingsDirectory}/{GlobalSettingsType.FullName}.asset"; }
        public SerializedObject serializedObject;
        public VisualElement rootElement;
        public Action<SerializedObject, VisualElement> onEnable;
        public Action<SerializedObject> onDisable;

        public ProjectGlobalSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        public override void OnDeactivate() {
            if (onDisable != null) {
                onDisable.Invoke(serializedObject);
            }
        }
        public override void OnActivate(string searchContext, VisualElement rootElement) {
            base.OnActivate(searchContext, rootElement);
            this.rootElement = rootElement;
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplateUxml).CloneTree(rootElement);
            rootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(TemplateUss));
            rootElement.Q<Label>("settings-name").text = this.label;
            if (File.Exists(AssetPath)) {
                var asset = AssetDatabase.LoadAssetAtPath(AssetPath, GlobalSettingsType);
                if (asset != null) {
                    serializedObject = new SerializedObject(asset);
                    FillSettings(rootElement, serializedObject);
                }
                else {
                    FillWithMissingSettings(rootElement);
                }
            }
            else {
                FillWithMissingSettings(rootElement);
            }
        }
        public void CreateSettingsUpdateView() {
            var asset = CreateSettings();
            serializedObject = new SerializedObject(asset);
            FillSettings(rootElement, serializedObject);
        }
        private void FillWithMissingSettings(VisualElement rootElement) {
            var container = rootElement.Q<VisualElement>("container");
            container.Clear();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MissingSettingsUxml).CloneTree(container);
            container.Q<Label>("text").text = $"Global settings asset {GlobalSettingsType.FullName} not found.";
            container.Q<Button>("create-settings").clicked += CreateSettingsUpdateView;
        }
        private void FillSettings(VisualElement rootElement, SerializedObject serializedObject) {
            var container = rootElement.Q<VisualElement>("container");
            container.Clear();
            if (string.IsNullOrWhiteSpace(uxmlPath)) {
                InspectorElement.FillDefaultInspector(container, serializedObject, null);
            }
            else {
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath).CloneTree(container);
                if (!string.IsNullOrWhiteSpace(ussPath)) {
                    container.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath));
                }
            }
            container.Bind(serializedObject);
            if (onEnable != null) {
                onEnable.Invoke(serializedObject, rootElement);
            }

        }
        public ScriptableObject CreateSettings() {
            if (!Directory.Exists($"{ProjectUtility.SettingsDirectory}")) {
                Directory.CreateDirectory($"{ProjectUtility.SettingsDirectory}");
            }
            var asset = ScriptableObject.CreateInstance(GlobalSettingsType);
            Debug.Log(AssetPath);
            AssetDatabase.CreateAsset(asset, AssetPath);
            return asset;
        }
    }
    public class ProjectGlobalSettingsProvider<TSettings> : ProjectGlobalSettingsProvider where TSettings : UnityEngine.Object {
        public override Type GlobalSettingsType => typeof(TSettings);


        public ProjectGlobalSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) {
        }
    }
}