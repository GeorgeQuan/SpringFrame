using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class MessageModule : BaseGameModule
{
    public delegate Task MessageHandlerEventArgs<T>(T arg);//��Ϣ�������¼�����  ���ͷ���һ��Task

    private Dictionary<Type, List<object>> globalMessageHandlers;//�Զ�ɨ����Ϣ
    private Dictionary<Type, List<object>> localMessageHandlers;//�������Ϣ

    public Monitor Monitor { get; private set; }//���������

    /// <summary>
    /// ��ʼ������
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();
        Monitor = new Monitor();
        LoadAllMessagHandlers();
    }
    /// <summary>
    /// ֹͣʱ������Ϣ����
    /// </summary>
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }
    /// <summary>
    ///����������Ϣ������
    /// </summary>
    private void LoadAllMessagHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())// ��ȡ���ó����е���������
        {
            if (type.IsAbstract)//����ǳ�������continue
                continue;
            //���ͻ�ȡ���� true �Ӽ̳����л�ȡ �оͷ���,û�з���null
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
            if (messageHandlerAttribute != null)//��������̳�MessageHandler
            {
                //����type ���͵�ʵ��,��ת��Ϊ�ӿ�����
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;  
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);//��ղŴ��������ʵ������ֵ�
            }
        }
    }
    /// <summary>
    /// ����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType = typeof(T);//�õ��ṹ������Ҳ��������
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))//�ж���������û��
        {
            handlerList = new List<object>();//û�о��½�
            localMessageHandlers.Add(argType, handlerList);
        }
        handlerList.Add(handler);//����¶���

    }
    /// <summary>
    /// ȡ������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))//�ж���û��������͵���Ϣ
            return;
        handlerList.Remove(handler);//�о��Ƴ���Ϣ
    }

    public async Task Post<T>(T arg) where T : struct
    {
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))//�ж�ȫ����Ϣ����û���������
        {
            foreach (var handler in globalHandlerList)//������������ҵ������Ϣ
            {
                if (!(handler is MessageHandler<T> messageHandler))//����ҵ��˾ʹ����messageHandler��
                    continue;

              await messageHandler.HandleMessage(arg);//�ȴ�������� �����������ִ������
               
            }
        }

        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))//�ж��Լ����ĵ���������û���������
        {
            List<object> list = ListPool<object>.Obtain();//�ӳ������õ����϶���
            list.AddRangeNonAlloc(localHandlerList);
           
            foreach (var handler in list)//�����������
            {
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg);//�ȴ�������� ʵʩ����������ȴ�����������ִ�����������߼�
            }
            ListPool<object>.Release(list);//����ȥ
        }
    }


}
