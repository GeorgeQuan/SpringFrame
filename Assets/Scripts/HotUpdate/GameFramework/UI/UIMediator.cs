using Config;
using Nirvana;
using System.Xml;
using UnityEngine;

// 泛型类 UIMediator<T> 继承自 UIMediator，T 必须是 UIView 的子类
public abstract class UIMediator<T> : UIMediator where T : UIView
{
    // 受保护的 T 类型的视图
    protected T view;

    // 重写 OnShow 方法
    protected override void OnShow(object arg)
    {
        base.OnShow(arg); // 调用基类的 OnShow 方法
        view = ViewObject.GetComponent<T>(); // 获取 T 类型的组件并赋值给 view
    }

    // 重写 OnHide 方法
    protected override void OnHide()
    {
        view = default; // 将 view 置为默认值 (null)
        base.OnHide(); // 调用基类的 OnHide 方法
    }

    // 关闭 UI
    protected void Close()
    {
        TGameFramework.Instance.GetModule<UIModule>().CloseUI(this); // 使用框架关闭当前 UI
    }

    // 初始化 Mediator
    public override void InitMediator(UIView view)
    {
        base.InitMediator(view); // 调用基类的 InitMediator 方法
        OnInit(view as T); // 调用 OnInit 方法并传入 T 类型的视图
    }

    // 受保护的虚方法，用于初始化 T 类型的视图，可以在子类中重写
    protected virtual void OnInit(T view) { }
}

// 基类 UIMediator
public abstract class UIMediator
{
    // 事件，在 Mediator 隐藏时触发
    public event System.Action OnMediatorHide;

    // 公有属性，UI 对应的游戏对象
    public GameObject ViewObject { get; set; }

    // 公有属性，UI 事件表
    public UIEventTable eventTable { get; set; }

    // 公有属性，UI 名称表
    public UINameTable nameTable { get; set; }

    // 公有属性，UI 的排序顺序
    public int SortingOrder { get; set; }

    // 公有属性，UI 的模式
    public UIMode UIMode { get; set; }

    // 虚方法，用于初始化 Mediator，可以在子类中重写
    public virtual void InitMediator(UIView view) { }

    // 显示 UI，并传入视图对象和参数
    public void Show(GameObject viewObject, object arg)
    {
        ViewObject = viewObject; // 设置视图对象
        eventTable = ViewObject.GetComponent<UIEventTable>(); // 获取 UI 事件表组件
        nameTable = viewObject.GetComponent<UINameTable>(); // 获取 UI 名称表组件
        OnShow(arg); // 调用虚方法 OnShow
    }

    // 受保护的虚方法，在显示时调用，可以在子类中重写
    protected virtual void OnShow(object arg) { }

    // 隐藏 UI
    public void Hide()
    {
        OnHide(); // 调用虚方法 OnHide
        OnMediatorHide?.Invoke(); // 触发 OnMediatorHide 事件
        OnMediatorHide = null; // 清空事件
        ViewObject = default; // 将视图对象置为默认值 (null)
    }

    // 受保护的虚方法，在隐藏时调用，可以在子类中重写
    protected virtual void OnHide() { }

    // 更新方法，传入 deltaTime
    public void Update(float deltaTime)
    {
        OnUpdate(deltaTime); // 调用虚方法 OnUpdate
    }

    // 受保护的虚方法，在更新时调用，可以在子类中重写
    protected virtual void OnUpdate(float deltaTime) { }
}
