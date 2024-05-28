using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 消息模块
    /// </summary>
    [Module(1)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    /// <summary>
    /// 流程 程序模块
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get=>TGameFramework.Instance.GetModule<ProcedureModule>(); }
    /// <summary>
    /// ui模块
    /// </summary>
    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    private bool activing;//是否在运行程序

    private void Awake()
    {
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        activing = true;//开始的时候在运行程序
        Application.logMessageReceived += OnReceiveLog;//添加打印日志的事件
        TGameFramework.Initialize();//启用单例
        StartupModules();
        TGameFramework.Instance.InitModules();//调用初始化模块方法
     
    }
    private void Start()
    {
        TGameFramework.Instance.StartModules();
        Procedure.StartProcedure().Coroutine();//开始游戏程序
    }

    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }
    private void OnDestroy()
    {
        if (activing)
        {
            Application.logMessageReceived -= OnReceiveLog; //这里是包的日志 -= 的是日志的方法
            TGameFramework.Instance.Destroy();//对TGameFrameWork 进行删除时的数据处理
        }
    }
    /// <summary>
    /// 初始化模块
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();//创建存储实现了特性的属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);//查找本类中所有符合要求的属性
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)//遍历所有的属性
        {
            PropertyInfo property = propertyInfos[i];//保存当前属性
            if (!baseCompType.IsAssignableFrom(property.PropertyType))//判断这个属性的类型是不是BaseGameModule 或继承额它
                continue;
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);//获取特性ModuleAttribute false 代表不寻找继承链
            if (attrs.Length == 0)//如果该属性没有这个特性,进行下一次
                continue;
            Component comp = GetComponentInChildren(property.PropertyType);//在当前游戏对象下查找类型为属性类型的组件
            if (comp == null)//说明这个模块没有被挂载在对象上
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;//拿到特性
            moduleAttr.Module = comp as BaseGameModule;//基于特性模块
            moduleAttrs.Add(moduleAttr);//添加进容器
        }
        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });//对所有模块的优先级进行排序
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }


    }



    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]//设置特性
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>//继承生成特性类,继承比较接口
    {
        /// <summary>
        /// 模块加载优先级
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
    /// <summary>
    /// 打印日志的方法 #if 后只在应用中执行,不在unity 中执行
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
            if (type == LogType.Exception)
            {
                UnityLog.Fatal($"{condition}\n{stackTrace}");
            }
#endif
    }
   
}
