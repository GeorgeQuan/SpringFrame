using Config;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UI模块
/// </summary>

public partial class UIModule : BaseGameModule
{
    public Transform normalUIRoot;//正常UI根
    public Transform modalUIRoot;//模块UI根
    public Transform closeUIRoot;//关闭UI根
    public Image imgMask;//图片遮罩
    public QuantumConsole prefabQuantumConsole;//量子控制器没用到

    private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING;//UIID 中介者类型
    private static Dictionary<UIViewID, Type> ASSET_MAPPING;//UIID  View类型

    private readonly List<UIMediator> usingMediators = new List<UIMediator>();//打开的中介者容器
    private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();//中介者池
    private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();//ui对象池
    private QuantumConsole quantumConsole;//量子控制台

    /// <summary>
    /// 初始化没用到
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        //quantumConsole = Instantiate(prefabQuantumConsole);
        //quantumConsole.transform.SetParentAndResetAll(transform);
        //quantumConsole.OnActivate += OnConsoleActive;
        //quantumConsole.OnDeactivate += OnConsoleDeactive;
    }
    /// <summary>
    /// 停止没用到
    /// </summary>
    protected internal override void OnModuleStop()
    {
        //base.OnModuleStop();
        //quantumConsole.OnActivate -= OnConsoleActive;
        //quantumConsole.OnDeactivate -= OnConsoleDeactive;
    }
    /// <summary>
    /// 缓存ui映射 ,在显示UI时先调用
    /// </summary>
    private static void CacheUIMapping()
    {
        if (MEDIATOR_MAPPING != null)//如果中介者容器已经不为空了,意味着已经执行过了,return
            return;

        MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();//初始化
        ASSET_MAPPING = new Dictionary<UIViewID, Type>();

        Type baseViewType = typeof(UIView);//拿到类型反射
        foreach (var type in baseViewType.Assembly.GetTypes())//获取类型所在的程序集内所有的类型
        {
            if (type.IsAbstract)//如果是抽象类就返回
                continue;

            if (baseViewType.IsAssignableFrom(type))//判断type 是不是baseViewType 的派生类
            {
                object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);//获取该类型指定特性,不包括子类
                if (attrs.Length == 0)//如果为空,没有特性,也就以为这没有绑定中介者
                {
                    UnityLog.Error($"{type.FullName} 没有绑定 Mediator，请使用UIMediatorAttribute绑定一个Mediator以正确使用");
                    continue;
                }

                foreach (UIViewAttribute attr in attrs)//遍历所有特性
                {
                    MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);//ID为键,存储中介者类型
                    ASSET_MAPPING.Add(attr.ID, type);//ID为键,存储View类型
                    break;
                }
            }
        }
    }
    /// <summary>
    /// upDate方法
    /// </summary>
    /// <param name="deltaTime"></param>

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        uiObjectPool.UpdateLoadRequests();//Update里面加载请求,里面会判断是否有请求
        foreach (var mediator in usingMediators)//遍历中介者容器
        {
            mediator.Update(deltaTime);//调用中介者的UPdate
        }
        UpdateMask(deltaTime);//调用遮罩
    }

    private void OnConsoleActive()
    {
        //GameManager.Input.SetEnable(false);
    }

    private void OnConsoleDeactive()
    {
        //GameManager.Input.SetEnable(true);
    }
    /// <summary>
    /// 获取当前模式下最高的渲染层级
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private int GetTopMediatorSortingOrder(UIMode mode)
    {
        int lastIndexMediatorOfMode = -1;
        for (int i = usingMediators.Count - 1; i >= 0; i--)//倒序遍历运行中介者容器
        {
            UIMediator mediator = usingMediators[i];//获取中介者 实例
            if (mediator.UIMode != mode)//判断uimode 是否匹配
                continue;

            lastIndexMediatorOfMode = i;//匹配上保存下标
            break;
        }

        if (lastIndexMediatorOfMode == -1)//-1 的话意味着没有这个模式正在运行的中介者
            return mode == UIMode.Normal ? 0 : 1000;//判断模式是不是normal 返回不同值

        return usingMediators[lastIndexMediatorOfMode].SortingOrder;//返回正在运行这个模式的中介者实例的渲染层级
    }
    /// <summary>
    /// 获取中介者
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private UIMediator GetMediator(UIViewID id)
    {
        CacheUIMapping();//先寻找中介者

        if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))//根据id 查找中介者类型
        {
            UnityLog.Error($"找不到 {id} 对应的Mediator");
            return null;
        }

        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//通过中介者类型查找(实例)
        {
            mediatorQ = new Queue<UIMediator>();//如果没有就创建中介者队列
            freeMediators.Add(mediatorType, mediatorQ);//存放进字典
        }

        UIMediator mediator;//创建基类类型
        if (mediatorQ.Count == 0)//如果创建的容器为空
        {
            mediator = Activator.CreateInstance(mediatorType) as UIMediator;//根据类型创建实例 返回
        }
        else
        {
            mediator = mediatorQ.Dequeue();//如果有直接出队 返回
        }

        return mediator;
    }
    /// <summary>
    /// 回收中介者
    /// </summary>
    /// <param name="mediator"></param>
    private void RecycleMediator(UIMediator mediator)//接收中介者对象
    {
        if (mediator == null)//判空
            return;

        Type mediatorType = mediator.GetType();//获取类型
        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//从中介者池中查找是否为空
        {
            mediatorQ = new Queue<UIMediator>();//为空就创建添加
            freeMediators.Add(mediatorType, mediatorQ);//添加进容器
        }
        mediatorQ.Enqueue(mediator);//入队
    }
    /// <summary>
    /// 获取打开的ui中介者
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UIMediator GetOpeningUIMediator(UIViewID id)//传入ID
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//从配置表内查找
        if (uiConfig.IsNull)//如果配置表中没有返回
            return null;

        UIMediator mediator = GetMediator(id);//根据ui获取中介者对象
        if (mediator == null)
            return null;

        Type requiredMediatorType = mediator.GetType();//拿到对象类型
        foreach (var item in usingMediators)//遍历中介者容器
        {
            if (item.GetType() == requiredMediatorType)//找到容器中的中介者 返回
                return item;
        }
        return null;
    }
    /// <summary>
    /// 使置顶
    /// </summary>
    /// <param name="id"></param>
    public void BringToTop(UIViewID id)//接收uiID
    {
        UIMediator mediator = GetOpeningUIMediator(id);//从打开的中介者容器内查找中介者实例
        if (mediator == null)//判空
            return;

        int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);//获取该模式下最上层的渲染层级
        if (mediator.SortingOrder == topSortingOrder)//如果已经是最高的 直接返回
            return;

        int sortingOrder = topSortingOrder + 10;//在当前最高层级的基础上在增加10,成为最高层架
        mediator.SortingOrder = sortingOrder;//赋值新排序数值

        usingMediators.Remove(mediator);//删除中介者

        usingMediators.Add(mediator);//重新添加进末尾

        Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();//获取Canvas
        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder;//修改Canvas 层级 也在最上面
        }
    }
    /// <summary>
    /// 判断ui是否打开了
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsUIOpened(UIViewID id)
    {
        return GetOpeningUIMediator(id) != null;//如果已经打开的中介者中没有会返回null
    }
    /// <summary>
    /// 使置顶重载
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public UIMediator BringToTop(UIViewID id, object arg = null)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator != null)
            return mediator;

        return OpenUI(id, arg);
    }
    /// <summary>
    /// 打开ui
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public UIMediator OpenUI(UIViewID id, object arg = null)//传入uiID
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//判断是否有配置
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);//返回中介者实例
        if (mediator == null)
            return null;
        //加载资源
        GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
        {
            //获取组件
            UIView newView = obj.GetComponent<UIView>();
            //中介者初始化
            mediator.InitMediator(newView);
        })).gameObject;
        return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
    }
    /// <summary>
    /// 异步打开UI (单例)只能打开一个
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
    {
        if (!IsUIOpened(id))//判断是否已经打开了
        {
            yield return OpenUIAsync(id, arg);//没打开调用打开方法
        }
    }
    /// <summary>
    /// 异步打开UI
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public IEnumerator OpenUIAsync(UIViewID id, object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//从配置表中查找
        if (uiConfig.IsNull)
            yield break;

        UIMediator mediator = GetMediator(id);//找中介者对象
        if (mediator == null)
            yield break;

        bool loadFinish = false;//加载是否完成bool
        //调用异步加载游戏对象方法
        uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
        {
            GameObject uiObject = asset.gameObject;
            OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
            loadFinish = true;//加载完成
        }, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();//获取到UIView
            mediator.InitMediator(newView);//初始化中介者
        });
        while (!loadFinish)
        {
            yield return null;
        }
        yield return null;
        yield return null;
        
    }
    /// <summary>
    /// UI对象加载后处理
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="uiConfig"></param>
    /// <param name="uiObject"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
    {
        if (uiObject == null)//判断ui对象是否为空
        {
            UnityLog.Error($"加载UI失败:{uiConfig.Asset}");
            RecycleMediator(mediator);//回收中介者
            return null;
        }

        UIView view = uiObject.GetComponent<UIView>();//获取ui对象View组件
        if (view == null)//判断是否为空
        {
            UnityLog.Error($"UI Prefab不包含UIView脚本:{uiConfig.Asset}");
            RecycleMediator(mediator);//回收中介者
            uiObjectPool.UnloadGameObject(view.gameObject);//不加在游戏对象,
            return null;
        }

        mediator.UIMode = uiConfig.Mode;//给中介者复制mode
        int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;//从新计算层级

        usingMediators.Add(mediator);//添加进容器

        Canvas canvas = uiObject.GetComponent<Canvas>();//获取对象的Canvax
        canvas.renderMode = RenderMode.ScreenSpaceCamera;//修改Canvas模式
        //canvas.worldCamera = GameManager.Camera.uiCamera;
        //根据不同的模式添加不同的根
        if (uiConfig.Mode == UIMode.Normal)
        {
            uiObject.transform.SetParentAndResetAll(normalUIRoot);
            canvas.sortingLayerName = "NormalUI";
        }
        else
        {
            uiObject.transform.SetParentAndResetAll(modalUIRoot);
            canvas.sortingLayerName = "ModalUI";
        }

        mediator.SortingOrder = sortingOrder;//赋值新的层级
        canvas.sortingOrder = sortingOrder;

        uiObject.SetActive(true);//激活游戏对象
        mediator.Show(uiObject, obj);//调用显示方法
        return mediator;//返回中介者对象
    }
    /// <summary>
    /// 关闭UI
    /// </summary>
    /// <param name="mediator"></param>
    public void CloseUI(UIMediator mediator)
    {
        if (mediator != null)
        {
            // 回收View
            uiObjectPool.UnloadGameObject(mediator.ViewObject);
            mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

            // 回收Mediator
            mediator.Hide();
            RecycleMediator(mediator);

            usingMediators.Remove(mediator);//从容器中删除中介者对象
        }
    }
    /// <summary>
    /// 关闭所有UI
    /// </summary>
    public void CloseAllUI()
    {
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            CloseUI(usingMediators[i]);
        }
    }
    /// <summary>
    /// 关闭指定UI
    /// </summary>
    /// <param name="id"></param>
    public void CloseUI(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator == null)
            return;

        CloseUI(mediator);
    }
    /// <summary>
    /// 设置所有这个类型的UI是否可见
    /// </summary>
    /// <param name="visible"></param>
    public void SetAllNormalUIVisibility(bool visible)
    {
        normalUIRoot.gameObject.SetActive(visible);
    }

    public void SetAllModalUIVisibility(bool visible)
    {
        modalUIRoot.gameObject.SetActive(visible);
    }
    /// <summary>
    /// 设置遮罩
    /// </summary>
    /// <param name="duration"></param>
    public void ShowMask(float duration = 0.5f)
    {
        destMaskAlpha = 1;
        maskDuration = duration;
    }

    public void HideMask(float? duration = null)
    {
        destMaskAlpha = 0;
        if (duration.HasValue)
        {
            maskDuration = duration.Value;
        }
    }
    //目标透明度
    private float destMaskAlpha = 0;
    //遮罩持续时间
    private float maskDuration = 0;
    private void UpdateMask(float deltaTime)
    {
        Color c = imgMask.color;//获取遮罩Image 颜色
        //如果持续时间没有结束
        //当前透过度,目标透明度,插值  如果到了直接赋值目标透明度
        c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
        c.a = Mathf.Clamp01(c.a);//约束到0-1
        imgMask.color = c;//设置遮罩颜色
        imgMask.enabled = imgMask.color.a > 0;//如果完全透明禁用遮罩
    }

    public void ShowConsole()
    {
        quantumConsole.Activate();//激活量子控制台
    }
}

/// <summary>
/// 自定义属性,创建新界面时保留UI的id 和中介者类型
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; }
    public Type MediatorType { get; }

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id;
        MediatorType = mediatorType;
    }
}
