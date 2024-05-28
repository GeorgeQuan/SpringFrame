using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginMessageHandler : MessageHandler<MessageType.Login>
{
    public override Task HandleMessage(MessageType.Login arg)
    {
        throw new System.NotImplementedException();
    }
}
