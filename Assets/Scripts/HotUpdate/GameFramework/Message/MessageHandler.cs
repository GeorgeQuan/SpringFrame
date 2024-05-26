using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IMessageHandler
{
    Type GetHandlerType();
}
[MessageHandler]
public abstract class MessageHandler<T> : IMessageHandler where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);//��ȡ����������
    }
    public abstract Task HandleMessage(T arg);
}
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]//������Ϣ����������
sealed class MessageHandlerAttribute : Attribute { }
