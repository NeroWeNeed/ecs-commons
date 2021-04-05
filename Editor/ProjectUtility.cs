using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class ProjectUtility {
        public const string AssetDirectory = "Assets/ProjectSettings/Assets";
        public const string SettingsDirectory = "Assets/ProjectSettings";
        private static readonly Dictionary<Type, ProjectGlobalSettings> settingsCache = new Dictionary<Type, ProjectGlobalSettings>();
        private static readonly Dictionary<Type, object> projectAssetCache = new Dictionary<Type, object>();
        private static readonly Dictionary<ProjectAssetType, BaseProjectAssetSerializer> projectAssetSerializers = new Dictionary<ProjectAssetType, BaseProjectAssetSerializer>();
        [InitializeOnLoadMethod]
        private static void CollectProjectAssetSerializers() {
            EditorApplication.projectChanged -= UpdateCache;
            EditorApplication.projectChanged += UpdateCache;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var attr in assembly.GetCustomAttributes<ProjectAssetSerializerAttribute>()) {
                    projectAssetSerializers[attr.type] = (BaseProjectAssetSerializer)Activator.CreateInstance(attr.serializer);
                }
            }
        }
        public static void UpdateCache() {
            settingsCache.Clear();
            projectAssetCache.Clear();
        }
        public static string GetProjectSettingsPath<TSettings>() => $"{ProjectUtility.SettingsDirectory}/{typeof(TSettings).FullName}.asset";
        public static string GetProjectSettingsPath(Type settingsType) => settingsType == null ? throw new ArgumentException("settingsType") : $"{ProjectUtility.SettingsDirectory}/{settingsType.FullName}.asset";
        public static ProjectGlobalSettings GetProjectSettings(Type settingsType, bool cache = true) {
            if (settingsType == null || !typeof(ProjectGlobalSettings).IsAssignableFrom(settingsType)) {
                throw new ArgumentException("settingsType");
            }
            ProjectGlobalSettings result;
            if (settingsCache.TryGetValue(settingsType, out result))
                return result;
            var path = GetProjectSettingsPath(settingsType);
            if (File.Exists(path)) {
                result = AssetDatabase.LoadAssetAtPath(path, settingsType) as ProjectGlobalSettings;
                if (result is IDeserializationCallback cb) {
                    cb.OnDeserialize();
                }
                if (cache) {
                    settingsCache[settingsType] = result;
                }
                return result;
            }
            else {
                return null;
            }

        }
        public static ProjectGlobalSettings GetOrCreateProjectSettings(Type settingsType, bool cache = true) {
            if (settingsType == null || !typeof(ProjectGlobalSettings).IsAssignableFrom(settingsType)) {
                throw new ArgumentException("settingsType");
            }
            ProjectGlobalSettings result;
            if (settingsCache.TryGetValue(settingsType, out result))
                return result;
            var path = GetProjectSettingsPath(settingsType);
            result = File.Exists(path) ? AssetDatabase.LoadAssetAtPath(path, settingsType) as ProjectGlobalSettings : ProjectGlobalSettings.CreateSettings(settingsType);
            if (result is IDeserializationCallback cb) {
                cb.OnDeserialize();
            }
            if (cache) {
                settingsCache[settingsType] = result;
            }
            return result;
        }
        public static TSettings GetProjectSettings<TSettings>(bool cache = true) where TSettings : ProjectGlobalSettings => (TSettings)GetProjectSettings(typeof(TSettings), cache);
        public static TSettings GetOrCreateProjectSettings<TSettings>(bool cache = true) where TSettings : ProjectGlobalSettings => (TSettings)GetOrCreateProjectSettings(typeof(TSettings), cache);

        public static string GetProjectAssetPath(Type projectAssetType) => $"{AssetDirectory}/{projectAssetType.FullName}.{GetProjectAssetExtension(projectAssetType)}";
        private static string GetProjectAssetExtension(Type projectAssetType) {
            if (projectAssetType == null) {
                throw new ArgumentNullException(nameof(projectAssetType));
            }
            if (typeof(ScriptableObject).IsAssignableFrom(projectAssetType)) {
                return "asset";
            }
            else {
                var attr = projectAssetType.GetCustomAttribute<ProjectAssetAttribute>();
                if (attr == null) {
                    throw new ArgumentException($"Type must be of {nameof(ScriptableObject)} or annotated with {nameof(ProjectAssetAttribute)}.");
                }
                return attr.extension;
            }
        }
        public static string GetProjectAssetPath<TProjectAsset>() => GetProjectAssetPath(typeof(TProjectAsset));

        public static object GetProjectAsset(Type projectAssetType, bool cache = true) {
            if (projectAssetType == null) {
                throw new ArgumentNullException(nameof(projectAssetType));
            }
            object result;
            if (projectAssetCache.TryGetValue(projectAssetType, out result)) {
                return result;
            }
            var path = GetProjectAssetPath(projectAssetType);
            if (File.Exists(path)) {
                result = LoadProjectAssetAtPath(path, projectAssetType);
                if (cache) {
                    projectAssetCache[projectAssetType] = result;
                }
                return result;
            }
            else {
                return null;
            }

        }
        private static object LoadProjectAssetAtPath(string path, Type projectAssetType) {
            object asset;
            if (typeof(ScriptableObject).IsAssignableFrom(projectAssetType)) {
                asset = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, projectAssetType);

            }
            else {
                var attr = projectAssetType.GetCustomAttribute<ProjectAssetAttribute>();
                if (attr == null) {
                    throw new ArgumentException($"Type must be of {nameof(ScriptableObject)} or annotated with {nameof(ProjectAssetAttribute)}.");
                }
                var serializer = projectAssetSerializers[attr.type];
                asset = serializer.Deserialize(projectAssetType, path);
            }
            if (asset is IDeserializationCallback cb) {
                cb.OnDeserialize();
            }
            return asset;
        }
        public static TProjectAsset GetProjectAsset<TProjectAsset>(bool cache = true) => (TProjectAsset)GetProjectAsset(typeof(TProjectAsset), cache);
        public static object GetOrCreateProjectAsset(Type projectAssetType, bool cache = true) {
            if (projectAssetType == null) {
                throw new ArgumentNullException(nameof(projectAssetType));
            }
            object result;
            if (projectAssetCache.TryGetValue(projectAssetType, out result)) {
                return result;
            }
            var path = GetProjectAssetPath(projectAssetType);
            if (File.Exists(path)) {
                result = LoadProjectAssetAtPath(path, projectAssetType);
            }
            else {
                if (typeof(ScriptableObject).IsAssignableFrom(projectAssetType)) {
                    result = ScriptableObject.CreateInstance(projectAssetType);
                    if (!Directory.Exists(AssetDirectory)) {
                        Directory.CreateDirectory(AssetDirectory);
                    }
                    if (result is IInitializable initializable) {
                        initializable.OnInit();
                    }
                    if (result is ISerializationCallback cb) {
                        cb.OnSerialize();
                    }
                    AssetDatabase.CreateAsset((ScriptableObject)result, path);
                }
                else {
                    var attr = projectAssetType.GetCustomAttribute<ProjectAssetAttribute>();
                    if (attr == null) {
                        throw new ArgumentException($"Type must be of {nameof(ScriptableObject)} or annotated with {nameof(ProjectAssetAttribute)}.");
                    }
                    var serializer = projectAssetSerializers[attr.type];
                    result = Activator.CreateInstance(projectAssetType);
                    if (result is IInitializable initializable) {
                        initializable.OnInit();
                    }
                    if (result is ISerializationCallback cb) {
                        cb.OnSerialize();
                    }
                    serializer.Serialize(projectAssetType, path, result);
                    AssetDatabase.ImportAsset(path);
                }
            }
            if (cache) {
                projectAssetCache[projectAssetType] = result;
            }
            return result;

        }
        public static TProjectAsset GetOrCreateProjectAsset<TProjectAsset>(bool cache = true) => (TProjectAsset)GetOrCreateProjectAsset(typeof(TProjectAsset), cache);

        public static void UpdateProjectAsset(Type projectAssetType, object asset) {
            projectAssetCache.Remove(projectAssetType);
            /*             if (projectAssetCache.ContainsKey(projectAssetType)) {
                            projectAssetCache[projectAssetType] = asset;
                        } */
            if (typeof(ScriptableObject).IsAssignableFrom(projectAssetType)) {
                if (asset is ISerializationCallback cb) {
                    cb.OnSerialize();
                }
                EditorUtility.SetDirty((ScriptableObject)asset);
            }
            else {
                var attr = projectAssetType.GetCustomAttribute<ProjectAssetAttribute>();
                if (attr == null) {
                    throw new ArgumentException($"Type must be of {nameof(ScriptableObject)} or annotated with {nameof(ProjectAssetAttribute)}.");
                }
                var serializer = projectAssetSerializers[attr.type];
                var path = GetProjectAssetPath(projectAssetType);
                if (!Directory.Exists(AssetDirectory)) {
                    Directory.CreateDirectory(AssetDirectory);
                }
                if (asset is ISerializationCallback cb) {
                    cb.OnSerialize();
                }
                serializer.Serialize(projectAssetType, path, asset);


            }

        }
        public static void UpdateProjectAsset<TProjectAsset>(TProjectAsset asset) => UpdateProjectAsset(typeof(TProjectAsset), asset);


    }

}