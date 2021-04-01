using System;
using System.Collections.Generic;
using System.Linq;
using NeroWeNeed.Commons.AssemblyAnalyzers.Editor;
using Newtonsoft.Json;

namespace NeroWeNeed.Commons.Editor {

    [ProjectAsset(ProjectAssetType.Json)]
    public class SerializableMemberCache : IInitializable {
        /* [JsonProperty("data")]
        private Dictionary<string, AssemblyData> assemblyData = new Dictionary<string, AssemblyData>(); */
        [JsonProperty("data")]
        public Dictionary<string, AssemblyData> assemblyData = new Dictionary<string, AssemblyData>();
        public List<string> typeFields = new List<string>();
        //public List<string> methodFields = new List<string>();
        [JsonIgnore]
        public DictionaryView<string, AssemblyData, string, List<SerializableType>> types;
/*         [JsonIgnore]
        public DictionaryView<string, AssemblyData, string, List<SerializableMethod>> methods; */
        public SerializableMemberCache() {
            types = new DictionaryView<string, AssemblyData, string, List<SerializableType>>(() => assemblyData, (old) => old.Values.SelectMany(a => a.types).SelectMany(a => a.Value.Select(b => (a.Key, b))).GroupBy(a => a.Key, a => a.b).ToDictionary(a => a.Key, a => a.ToList()));
            //methods = new DictionaryView<string, AssemblyData, string, List<SerializableMethod>>(() => assemblyData, (old) => old.Values.SelectMany(a => a.methods).SelectMany(a => a.Value.Select(b => (a.Key, b))).GroupBy(a => a.Key, a => a.b).ToDictionary(a => a.Key, a => a.ToList()));
        }
        public void OnInit() {
            AssemblyAnalyzer.AnalyzeAssemblies(new SerializableMemberFinder() { cache = this });
        }
        public class AssemblyData {
            public Dictionary<string, List<SerializableType>> types = new Dictionary<string, List<SerializableType>>();
            //public Dictionary<string, List<SerializableMethod>> methods = new Dictionary<string, List<SerializableMethod>>();
        }

    }

}