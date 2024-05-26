namespace UnityEngine
{
    public static class GameObjectExt
    {
        /// <summary>
        /// ��GameObject ��Ӹı�㼶����չ����
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
