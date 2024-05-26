namespace UnityEngine
{
    public static class GameObjectExt
    {
        /// <summary>
        /// 给GameObject 添加改变层级的拓展方法
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerRecursive(this GameObject go, int layer)
        {
            foreach (var trans in go.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = layer;
            }
        }
    }
}
