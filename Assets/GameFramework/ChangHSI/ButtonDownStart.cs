using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ButtonDownStart : MessageHandler<MessageType.ButtonDown>
{
    public override async Task HandleMessage(MessageType.ButtonDown arg)
    {
        Debug.Log("我也不知道为什么");
        GameManager.Message.Subscribe<MessageType.ButtonDown>(async (msg) => { Debug.Log("王喆笑嘻嘻"); });
        await Task.Yield();
    }
}
