using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace NeroWeNeed.Commons.Editor {

    public abstract class ProjectGlobalSettings : ScriptableObject {
        internal static ProjectGlobalSettings CreateSettings(Type type) {
            if (!Directory.Exists(ProjectUtility.SettingsDirectory)) {
                Directory.CreateDirectory(ProjectUtility.SettingsDirectory);
            }
            var path = $"{ProjectUtility.SettingsDirectory}/{type.FullName}.asset";
            var asset = ScriptableObject.CreateInstance(type) as ProjectGlobalSettings;
            if (asset is IInit initializable) {
                initializable.Init();
            }
            AssetDatabase.CreateAsset(asset, path);
            return asset;
            
        }
    }
}