using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ButtonDownStart : MessageHandler<MessageType.ButtonDown>
{
    public override async Task HandleMessage(MessageType.ButtonDown arg)
    {
        Debug.Log("��Ҳ��֪��Ϊʲô");
        GameManager.Message.Subscribe<MessageType.ButtonDown>(async (msg) => { Debug.Log("����Ц����"); });
        await Task.Yield();
    }
}
