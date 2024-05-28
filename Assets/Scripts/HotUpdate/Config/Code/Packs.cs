using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
namespace Config
{
    public partial struct Packs
    {
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/Packs.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;
            datas = new List<Packs>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                Packs data = (Packs)dataObject.ToObject(typeof(Packs));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/Packs.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<Packs>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        Packs data = (Packs)dataObject.ToObject(typeof(Packs));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/Packs.bytes".ToLower();
            string fullBundleName = $"{directory}/{bundleName}";
            string assetName = $"assets/{bundleName}";
            #if UNITY_WEBGL && !UNITY_EDITOR
            UnityEngine.AssetBundle bundle = null;
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(fullBundleName);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.LogError(request.error);
            }
            else
            {
                bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            }
            #else
            yield return null;
            UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile($"{fullBundleName}", 0, 0);
            #endif
            UnityEngine.TextAsset ta = bundle.LoadAsset<UnityEngine.TextAsset>($"{assetName}");
            string json = ta.text;
            datas = new List<Packs>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                Packs data = (Packs)dataObject.ToObject(typeof(Packs));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static int Count;
        private static List<Packs> datas;
        private static Dictionary<int, int> indexMap;
        public static Packs ByID(int id)
        {
            if (id <= 0)
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))
            {
                throw new System.Exception($"Packs找不到ID:{id}");
            }
            return ByIndex(index);
        }
        public static Packs ByIndex(int index)
        {
            return datas[index];
        }
        public bool IsNull { get; private set; }
        public static Packs Null { get; } = new Packs() { IsNull = true }; 
        public System.Int32 ID { get; set; }
        public string Description { get; set; }
        public string name1 { get; set; }
        public System.Int32 num1 { get; set; }
        public System.Int32 bing1 { get; set; }
        public System.Int32 rate1 { get; set; }
        public string name2 { get; set; }
        public System.Int32 num2 { get; set; }
        public System.Int32 bing2 { get; set; }
        public System.Int32 rate2 { get; set; }
        public string name3 { get; set; }
        public System.Int32 num3 { get; set; }
        public System.Int32 bing3 { get; set; }
        public System.Int32 rate3 { get; set; }
        public string name4 { get; set; }
        public System.Int32 num4 { get; set; }
        public System.Int32 bing4 { get; set; }
        public System.Int32 rate4 { get; set; }
        public string name5 { get; set; }
        public System.Int32 num5 { get; set; }
        public System.Int32 bing5 { get; set; }
        public System.Int32 rate5 { get; set; }
    }
}
