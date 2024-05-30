using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
namespace Config
{
    public partial struct UIConfig
    {
        /// <summary>
        /// 按照地址反序列话
        /// </summary>
        /// <param name="directory"></param>
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/UIConfig.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;//获取json 数据
            datas = new List<UIConfig>();
            indexMap = new Dictionary<int, int>();//创建容器
            JArray array = JArray.Parse(json);//传入的 JSON 字符串 json 解析成一个 JArray 对象数组
            Count = array.Count;//保存数量
            for (int i = 0; i < array.Count; i++)//遍历数组
            {
                JObject dataObject = array[i] as JObject;//获取json对象
                UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));//json 对象改变类型
                datas.Add(data);//添加进容器
                indexMap.Add(data.ID, i);//存储ID 索引
            }
        }
        /// <summary>
        /// 按照文件反序列化
        /// </summary>
        /// <param name="directory"></param>
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/UIConfig.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<UIConfig>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/UIConfig.bytes".ToLower();
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
            datas = new List<UIConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                UIConfig data = (UIConfig)dataObject.ToObject(typeof(UIConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static int Count;//数量
        private static List<UIConfig> datas;//所有UIConfig 数据
        private static Dictionary<int, int> indexMap;//根据ID 找索引
        /// <summary>
        /// 查找ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static UIConfig ByID(int id)
        {
            if (id <= 0)//判断传入的数据
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))//从容器内查找
            {
                throw new System.Exception($"UIConfig找不到ID:{id}");
            }
            return ByIndex(index);//找到返回
        }
        public static UIConfig ByIndex(int index)
        {
            return datas[index];//找到返回数据
        }
        public bool IsNull { get; private set; }
        public static UIConfig Null { get; } = new UIConfig() { IsNull = true }; 
        public System.Int32 ID { get; set; }
        public string Description { get; set; }
        public string Asset { get; set; }
        public UIMode Mode { get; set; }
    }
}
