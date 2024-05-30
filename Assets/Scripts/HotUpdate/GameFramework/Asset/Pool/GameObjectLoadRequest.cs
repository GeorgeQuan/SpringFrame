using System;
using UnityEngine;

namespace TGame.Asset
{
    public class GameObjectLoadRequest<T> where T : GameObjectPoolAsset
    {
        public GameObjectLoadState State { get; private set; }//状态
        public string Path { get; }//地址
        public Action<GameObject> CreateNewCallback { get; }//创建新回调

        private Action<T> callback;//回调函数

        public GameObjectLoadRequest(string path, Action<T> callback, Action<GameObject> createNewCallback)//构造函数
        {
            Path = path;
            this.callback = callback;
            CreateNewCallback = createNewCallback;
        }

        public void LoadFinish(T obj)
        {
            if (State == GameObjectLoadState.Loading)//判断状态是否是正在加载
            {
                callback?.Invoke(obj);//执行消息回调方法
                State = GameObjectLoadState.Finish;//修改状态为完成
            }
        }
    }
}
