using Nirvana;
using UnityEngine;

[RequireComponent(typeof(UIEventTable))]//添加需要的组件
[RequireComponent(typeof(UINameTable))]
public abstract class UIView : MonoBehaviour
{
    public void OnServerInitialized()
    {
        UIEventTable eventTable = GetComponent<UIEventTable>();
        eventTable.ListenEvent("wnag", (a) => { });
        UINameTable name=GetComponent<UINameTable>();
        name.Find("lll");
    }
}