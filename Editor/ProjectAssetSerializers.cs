using System;
using System.IO;
using System.Xml.Serialization;
using NeroWeNeed.Commons.Editor;
using Newtonsoft.Json;

[assembly: ProjectAssetSerializer(NeroWeNeed.Commons.ProjectAssetType.Xml, typeof(XmlProjectAssetSerializer))]
[assembly: ProjectAssetSerializer(NeroWeNeed.Commons.ProjectAssetType.Json, typeof(JsonProjectAssetSerializer))]

namespace NeroWeNeed.Commons.Editor {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ProjectAssetSerializerAttribute : Attribute {

        public ProjectAssetType type;
        public Type serializer;
        public ProjectAssetSerializerAttribute(ProjectAssetType type, Type serializer) {
            this.type = type;
            this.serializer = serializer;
        }
    }
    public abstract class BaseProjectAssetSerializer {
        public abstract object Deserialize(Type projectAssetType, string path);
        public TAsset Deserialize<TAsset>(string path) => (TAsset)Deserialize(typeof(TAsset), path);
        public abstract void Serialize(Type projectAssetType, string path, object asset);
        public void Serialize<TAsset>(string path, TAsset asset) => Serialize(typeof(TAsset), path, asset);
    }
    public class XmlProjectAssetSerializer : BaseProjectAssetSerializer {
        public override object Deserialize(Type projectAssetType, string path) {
            var serializer = new XmlSerializer(projectAssetType);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return serializer.Deserialize(stream);
        }
        public override void Serialize(Type projectAssetType, string path, object asset) {
            var serializer = new XmlSerializer(projectAssetType);
            using var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            serializer.Serialize(stream, asset);
        }
    }
    public class JsonProjectAssetSerializer : BaseProjectAssetSerializer {
        public override object Deserialize(Type projectAssetType, string path) {
            var serializer = new JsonSerializer();
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            return serializer.Deserialize(jsonReader, projectAssetType);
        }
        public override void Serialize(Type projectAssetType, string path, object asset) {
            var serializer = new JsonSerializer();
            using var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            using var streamReader = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(streamReader);
            serializer.Serialize(jsonWriter, asset, projectAssetType);
        }
    }
}