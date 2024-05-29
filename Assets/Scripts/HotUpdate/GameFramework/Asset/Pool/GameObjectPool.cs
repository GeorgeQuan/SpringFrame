using System;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class GameObjectPool<T> where T : GameObjectPoolAsset
{
    private readonly Dictionary<int, Queue<T>> gameObjectPool = new Dictionary<int, Queue<T>>();
    private readonly List<GameObjectLoadRequest<T>> requests = new List<GameObjectLoadRequest<T>>();
    private readonly Dictionary<int, GameObject> usingObjects = new Dictionary<int, GameObject>();
    /// <summary>
    /// 加载对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="createNewCallback"></param>
    /// <returns></returns>
    public T LoadGameObject(string path, Action<GameObject> createNewCallback = null)
    {
        int hash = path.GetHashCode();//获取地址的哈希值
        if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))//从容器中找没有就添加
        {
            q = new Queue<T>();
            gameObjectPool.Add(hash, q);
        }
        if (q.Count == 0)//判断容器是否为空
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();//为空根据地址加载新资源  等待完成异步
            GameObject go = UnityEngine.Object.Instantiate(prefab);//复制
            T asset = go.AddComponent<T>();//对应类型的组件
            createNewCallback?.Invoke(go);//实施委托并传入参数
            asset.ID = hash;//资源ID 为哈希值
            go.SetActive(false);//失活
            q.Enqueue(asset);//入队
        }

        {
            T asset = q.Dequeue();//出队拿到对象
            OnGameObjectLoaded(asset);//添加对象
            return asset;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">需要加载的资源的路径</param>
    /// <param name="callback">每次调用LoadGameObjectAsync，无论是否从缓存里取出的，都会通过这个回调进行通知</param>
    /// <param name="createNewCallback">游戏对象第一次被克隆后调用，对象池取出的复用游戏对象，不会回调</param>
    public void LoadGameObjectAsync(string path, Action<T> callback, Action<GameObject> createNewCallback = null)
    {
        GameObjectLoadRequest<T> request = new GameObjectLoadRequest<T>(path, callback, createNewCallback);
        requests.Add(request);
    }

    public void UnloadAllGameObjects()
    {
        // 先将所有Request加载完毕
        while (requests.Count > 0)
        {
            //GameManager.Asset.UpdateLoader();
            UpdateLoadRequests();
        }

        // 将所有using Objects 卸载
        if (usingObjects.Count > 0)
        {
            List<int> list = new List<int>();
            foreach (var id in usingObjects.Keys)
            {
                list.Add(id);
            }
            foreach (var id in list)
            {
                GameObject obj = usingObjects[id];
                UnloadGameObject(obj);
            }
        }

        // 将所有缓存清掉
        if (gameObjectPool.Count > 0)
        {
            foreach (var q in gameObjectPool.Values)
            {
                foreach (var asset in q)
                {
                    UnityEngine.Object.Destroy(asset.gameObject);
                }
                q.Clear();
            }
            gameObjectPool.Clear();
        }
    }

    public void UnloadGameObject(GameObject go)
    {
        if (go == null)
            return;

        T asset = go.GetComponent<T>();
        if (asset == null)
        {
            UnityLog.Warn($"Unload GameObject失败，找不到GameObjectAsset:{go.name}");
            UnityEngine.Object.Destroy(go);
            return;
        }

        if (!gameObjectPool.TryGetValue(asset.ID, out Queue<T> q))//判断有无
        {
            q = new Queue<T>();
            gameObjectPool.Add(asset.ID, q);
        }
        q.Enqueue(asset);//入队
        usingObjects.Remove(go.GetInstanceID());//删除失活
        go.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().releaseObjectRoot);//设置父类
        go.gameObject.SetActive(false);//失活对象
    }

    public void UpdateLoadRequests()
    {
        if (requests.Count > 0)//判断是否有消息
        {
            foreach (var request in requests)//遍历消息容器
            {
                int hash = request.Path.GetHashCode();//获取哈希值
                if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))//判断池内有没有同样的哈希值
                {
                    q = new Queue<T>();
                    gameObjectPool.Add(hash, q);//没有就创建添加
                }

                if (q.Count == 0)//有实例但为0
                {
                    Addressables.LoadAssetAsync<GameObject>(request.Path).Completed += (obj) =>
                    {
                        GameObject go = UnityEngine.Object.Instantiate(obj.Result);
                        T asset = go.AddComponent<T>();
                        request.CreateNewCallback?.Invoke(go);
                        asset.ID = hash;
                        go.SetActive(false);

                        OnGameObjectLoaded(asset);
                        request.LoadFinish(asset);
                    };
                }
                else
                {
                    T asset = q.Dequeue();
                    OnGameObjectLoaded(asset);
                    request.LoadFinish(asset);
                }
            }

            requests.Clear();
        }
    }

    private void OnGameObjectLoaded(T asset)
    {
        asset.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().usingObjectRoot);//资源设置父类加载根
        int id = asset.gameObject.GetInstanceID();//获取实例ID
        usingObjects.Add(id, asset.gameObject);//添加进字典
    }
}

