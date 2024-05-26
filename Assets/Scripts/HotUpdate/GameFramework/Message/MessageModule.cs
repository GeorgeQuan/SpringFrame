using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class MessageModule : BaseGameModule
{
    public delegate Task MessageHandlerEventArgs<T>(T arg);//消息处理器事件参数  泛型返回一个Task

    private Dictionary<Type, List<object>> globalMessageHandlers;//自动扫描消息
    private Dictionary<Type, List<object>> localMessageHandlers;//后添加消息

    public Monitor Monitor { get; private set; }//定义监听器

    /// <summary>
    /// 初始化方法
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();
        Monitor = new Monitor();
        LoadAllMessagHandlers();
    }
    /// <summary>
    /// 停止时销毁消息容器
    /// </summary>
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }
    /// <summary>
    ///加载所有消息处理器
    /// </summary>
    private void LoadAllMessagHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())// 获取调用程序集中的所有类型
        {
            if (type.IsAbstract)//如果是抽象类型continue
                continue;
            //类型获取特性 true 从继承链中获取 有就返回,没有返回null
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
            if (messageHandlerAttribute != null)//带表其类继承MessageHandler
            {
                //创建type 类型的实例,并转换为接口类型
                IMessageHandler messageHandler = Activator.CreateInstance(type) as IMessageHandler;  
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);//帮刚才创建的类的实例存进字典
            }
        }
    }
    /// <summary>
    /// 订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType = typeof(T);//拿到结构体类型也就是名字
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))//判断容器内有没有
        {
            handlerList = new List<object>();//没有就新建
            localMessageHandlers.Add(argType, handlerList);
        }
        handlerList.Add(handler);//添加新订阅

    }
    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))//判断有没有这个类型的消息
            return;
        handlerList.Remove(handler);//有就移除消息
    }

    public async Task Post<T>(T arg) where T : struct
    {
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))//判断全局消息内有没有这个类型
        {
            foreach (var handler in globalHandlerList)//遍历这个容器找到这个消息
            {
                if (!(handler is MessageHandler<T> messageHandler))//如果找到了就存放在messageHandler中
                    continue;

              await messageHandler.HandleMessage(arg);//等待这个任务 任务结束继续执行下面
               
            }
        }

        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))//判断自己订阅的容器内有没有这个类型
        {
            List<object> list = ListPool<object>.Obtain();//从池里面拿到集合对象
            list.AddRangeNonAlloc(localHandlerList);
           
            foreach (var handler in list)//遍历这个容器
            {
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg);//等待这个任务 实施这个方法并等待它结束继续执行任务下面逻辑
            }
            ListPool<object>.Release(list);//换回去
        }
    }


}
