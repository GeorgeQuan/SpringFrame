using Nirvana;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIEventTable))]//添加需要的组件
[RequireComponent(typeof(UINameTable))]
public abstract class UIView : MonoBehaviour
{
    public void OnServerInitialized()
    {
        UIEventTable eventTable = GetComponent<UIEventTable>();
        eventTable.ListenEvent("wang", (a) => { });
        UINameTable name = GetComponent<UINameTable>();
        GameObject Button = name.Find("lll");
        Button.GetComponent<Button>().onClick.AddListener(() => { Debug.Log("王喆笑嘻嘻"); });

    } 
}