using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class ProjectUtility {
        public const string SingletonDirectory = "Assets/ProjectSettings/Data";
        public const string SettingsDirectory = "Assets/ProjectSettings";
        private static readonly Dictionary<Type, ProjectGlobalSettings> settingsCache = new Dictionary<Type, ProjectGlobalSettings>();
        private static readonly Dictionary<Type, ScriptableObject> singletonCache = new Dictionary<Type, ScriptableObject>();
        [InitializeOnLoadMethod]
        private static void ClearCaches() {
            settingsCache.Clear();
            singletonCache.Clear();
        }
        public static string GetSettingsPath<TSettings>() => $"{ProjectUtility.SettingsDirectory}/{typeof(TSettings).FullName}.asset";
        public static string GetSettingsPath(Type settingsType) => settingsType == null ? throw new ArgumentException("settingsType") : $"{ProjectUtility.SettingsDirectory}/{settingsType.FullName}.asset";
        public static ProjectGlobalSettings GetSettings(Type settingsType, bool cache = true) {
            if (settingsType == null || !typeof(ProjectGlobalSettings).IsAssignableFrom(settingsType)) {
                throw new ArgumentException("settingsType");
            }
            ProjectGlobalSettings result;
            if (settingsCache.TryGetValue(settingsType, out result))
                return result;
            var path = GetSettingsPath(settingsType);
            if (File.Exists(path)) {
                result = AssetDatabase.LoadAssetAtPath(path, settingsType) as ProjectGlobalSettings;
                if (cache) {
                    settingsCache[settingsType] = result;
                }
                return result;
            }
            else {
                return null;
            }

        }
        public static ProjectGlobalSettings GetOrCreateSettings(Type settingsType, bool cache = true) {
            if (settingsType == null || !typeof(ProjectGlobalSettings).IsAssignableFrom(settingsType)) {
                throw new ArgumentException("settingsType");
            }
            ProjectGlobalSettings result;
            if (settingsCache.TryGetValue(settingsType, out result))
                return result;
            var path = GetSettingsPath(settingsType);
            result = File.Exists(path) ? AssetDatabase.LoadAssetAtPath(path, settingsType) as ProjectGlobalSettings : ProjectGlobalSettings.CreateSettings(settingsType);
            if (cache) {
                settingsCache[settingsType] = result;
            }
            return result;
        }
        public static TSettings GetSettings<TSettings>(bool cache = true) where TSettings : ProjectGlobalSettings => (TSettings)GetSettings(typeof(TSettings), cache);
        public static TSettings GetOrCreateSettings<TSettings>(bool cache = true) where TSettings : ProjectGlobalSettings => (TSettings)GetOrCreateSettings(typeof(TSettings), cache);
        public static string GetSingletonPath(Type singletonType) => $"{SingletonDirectory}/{singletonType.FullName}.asset";
        public static string GetSingletonPath<TSingleton>() => GetSingletonPath(typeof(TSingleton));
        public static ScriptableObject GetSingleton(Type singletonType, bool cache = true) {
            if (!typeof(ScriptableObject).IsAssignableFrom(singletonType) || singletonType == null) {
                throw new ArgumentException("Type not ScriptableObject.");
            }
            ScriptableObject result;
            if (singletonCache.TryGetValue(singletonType, out result)) {
                return result;
            }
            var path = GetSingletonPath(singletonType);
            if (File.Exists(path)) {
                result = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, singletonType);
                if (cache) {
                    singletonCache[singletonType] = result;
                }
                return result;
            }
            else {
                return null;
            }

        }
        public static TSingleton GetSingleton<TSingleton>(bool cache = true) where TSingleton : ScriptableObject => (TSingleton)GetSingleton(typeof(TSingleton), cache);
        public static ScriptableObject GetOrCreateSingleton(Type singletonType, bool cache = true) {
            if (!typeof(ScriptableObject).IsAssignableFrom(singletonType) || singletonType == null) {
                throw new ArgumentException("Type not ScriptableObject.");
            }
            ScriptableObject result;
            if (singletonCache.TryGetValue(singletonType, out result)) {
                return result;
            }
            var path = GetSingletonPath(singletonType);
            if (File.Exists(path)) {
                result = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, singletonType);
            }
            else {
                result = ScriptableObject.CreateInstance(singletonType);
                if (!Directory.Exists(SingletonDirectory)) {
                    Directory.CreateDirectory(SingletonDirectory);
                }
                if (result is IInit initializable) {
                    initializable.Init();
                }

                AssetDatabase.CreateAsset(result, path);

            }
            if (cache) {
                singletonCache[singletonType] = result;
            }
            return result;

        }
        public static TSingleton GetOrCreateSingleton<TSingleton>(bool cache = true) where TSingleton : ScriptableObject => (TSingleton)GetOrCreateSingleton(typeof(TSingleton), cache);
    }
}