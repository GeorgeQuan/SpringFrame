using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginMessageHandler : MessageHandler<MessageType.Login>
{
    public async override Task HandleMessage(MessageType.Login arg)
    {
        await GameManager.UI.OpenUIAsync(UIViewID.LoginUI);
    }
}
