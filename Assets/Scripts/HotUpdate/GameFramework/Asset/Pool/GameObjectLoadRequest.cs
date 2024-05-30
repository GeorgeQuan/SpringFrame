using System;
using UnityEngine;

namespace TGame.Asset
{
    public class GameObjectLoadRequest<T> where T : GameObjectPoolAsset
    {
        public GameObjectLoadState State { get; private set; }//״̬
        public string Path { get; }//��ַ
        public Action<GameObject> CreateNewCallback { get; }//�����»ص�

        private Action<T> callback;//�ص�����

        public GameObjectLoadRequest(string path, Action<T> callback, Action<GameObject> createNewCallback)//���캯��
        {
            Path = path;
            this.callback = callback;
            CreateNewCallback = createNewCallback;
        }

        public void LoadFinish(T obj)
        {
            if (State == GameObjectLoadState.Loading)//�ж�״̬�Ƿ������ڼ���
            {
                callback?.Invoke(obj);//ִ����Ϣ�ص�����
                State = GameObjectLoadState.Finish;//�޸�״̬Ϊ���
            }
        }
    }
}
