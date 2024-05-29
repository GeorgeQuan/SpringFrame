using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class AssetModule : BaseGameModule
{
#if UNITY_EDITOR
    //[XLua.BlackList]
    public const string BUNDLE_LOAD_NAME = "Tools/Build/Bundle Load";
#endif

    public Transform usingObjectRoot;//使用根
    public Transform releaseObjectRoot;//删除根

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        UpdateGameObjectRequests();
    }

    public T LoadAsset<T>(string path) where T : Object
    {
        return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    }

    public void LoadObjectAsync<T>(string path, AssetLoadTask.OnLoadFinishEventHandler onLoadFinish) where T : UnityEngine.Object
    {
        Addressables.LoadAssetAsync<T>(path).Completed += (obj) =>
        {
            onLoadFinish?.Invoke(obj.Result);
        };
    }
}
