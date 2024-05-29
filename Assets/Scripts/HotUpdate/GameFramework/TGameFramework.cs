using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TGameFramework
{
    public static TGameFramework Instance { get; private set; }

    public static bool Initialized { get; private set; }

    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();//存放所有模块

    /// <summary>
    /// 初始化 用于单例
    /// </summary>
    public static void Initialize()
    {
        Instance = new TGameFramework();
    }

    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))//寻找模块有就返回
        {
            return module as T;
        }
        return default(T);//返回类型的默认值,在泛型方法中使用
    }
    /// <summary>
    /// 初始化模块
    /// </summary>
    public void InitModules()
    {
        if (Initialized)//如果模块已经初始化过了返回
            return;

        Initialized = true;
        foreach (var module in m_modules.Values)//遍历所有的模块
        {
            module.OnModuleInit();//调用模块的初始化方法
        }
    }
    /// <summary>
    /// 添加模块
    /// </summary>
    /// <param name="module"></param>
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
           // UnityLog.Info($"Module添加失败重复:{moduleType}");
            return;
        }
        m_modules.Add(moduleType, module);//存入模块容器
    }


    internal void StartModules()
    {
        if (m_modules == null)//如果存放模块的容器是空,返回
            return;
        if (!Initialized)//如果没有引用返回
            return;
        foreach (var module in m_modules.Values)//遍历所有模块
        {
            module.OnModuleStart();//调用模块Start方法
        }

    }

    internal void Update()
    {
        if (!Initialized)//如果没有初始化过返回
            return;
        if (m_modules == null)//没有模块返回
            return;

        if (!Initialized)//再次判断
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleUpdate(deltaTime);//调用所有的Updata方法
        }


    }

    internal void LateUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleLateUpdate(deltaTime);
        }
    }

    internal void FixedUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.fixedDeltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleFixedUpdate(deltaTime);
        }
    }

    internal void Destroy()
    {
        if (!Initialized)
            return;

        if (Instance != this)
            return;

        if (Instance.m_modules == null)
            return;

        foreach (var module in Instance.m_modules.Values)
        {
            module.OnModuleStop();//调用所有模块的停止方法
        }

        //Destroy(Instance.gameObject);
        Instance = null;//清空单例实例
        Initialized = false;//初始化设置为未初始化
    }
}
