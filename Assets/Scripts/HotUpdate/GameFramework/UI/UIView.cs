using Nirvana;
using UnityEngine;

[RequireComponent(typeof(UIEventTable))]//�����Ҫ�����
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