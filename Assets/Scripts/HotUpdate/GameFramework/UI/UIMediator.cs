using Config;
using Nirvana;
using System.Xml;
using UnityEngine;

// ������ UIMediator<T> �̳��� UIMediator��T ������ UIView ������
public abstract class UIMediator<T> : UIMediator where T : UIView
{
    // �ܱ����� T ���͵���ͼ
    protected T view;

    // ��д OnShow ����
    protected override void OnShow(object arg)
    {
        base.OnShow(arg); // ���û���� OnShow ����
        view = ViewObject.GetComponent<T>(); // ��ȡ T ���͵��������ֵ�� view
    }

    // ��д OnHide ����
    protected override void OnHide()
    {
        view = default; // �� view ��ΪĬ��ֵ (null)
        base.OnHide(); // ���û���� OnHide ����
    }

    // �ر� UI
    protected void Close()
    {
        TGameFramework.Instance.GetModule<UIModule>().CloseUI(this); // ʹ�ÿ�ܹرյ�ǰ UI
    }

    // ��ʼ�� Mediator
    public override void InitMediator(UIView view)
    {
        base.InitMediator(view); // ���û���� InitMediator ����
        OnInit(view as T); // ���� OnInit ���������� T ���͵���ͼ
    }

    // �ܱ������鷽�������ڳ�ʼ�� T ���͵���ͼ����������������д
    protected virtual void OnInit(T view) { }
}

// ���� UIMediator
public abstract class UIMediator
{
    // �¼����� Mediator ����ʱ����
    public event System.Action OnMediatorHide;

    // �������ԣ�UI ��Ӧ����Ϸ����
    public GameObject ViewObject { get; set; }

    // �������ԣ�UI �¼���
    public UIEventTable eventTable { get; set; }

    // �������ԣ�UI ���Ʊ�
    public UINameTable nameTable { get; set; }

    // �������ԣ�UI ������˳��
    public int SortingOrder { get; set; }

    // �������ԣ�UI ��ģʽ
    public UIMode UIMode { get; set; }

    // �鷽�������ڳ�ʼ�� Mediator����������������д
    public virtual void InitMediator(UIView view) { }

    // ��ʾ UI����������ͼ����Ͳ���
    public void Show(GameObject viewObject, object arg)
    {
        ViewObject = viewObject; // ������ͼ����
        eventTable = ViewObject.GetComponent<UIEventTable>(); // ��ȡ UI �¼������
        nameTable = viewObject.GetComponent<UINameTable>(); // ��ȡ UI ���Ʊ����
        OnShow(arg); // �����鷽�� OnShow
    }

    // �ܱ������鷽��������ʾʱ���ã���������������д
    protected virtual void OnShow(object arg) { }

    // ���� UI
    public void Hide()
    {
        OnHide(); // �����鷽�� OnHide
        OnMediatorHide?.Invoke(); // ���� OnMediatorHide �¼�
        OnMediatorHide = null; // ����¼�
        ViewObject = default; // ����ͼ������ΪĬ��ֵ (null)
    }

    // �ܱ������鷽����������ʱ���ã���������������д
    protected virtual void OnHide() { }

    // ���·��������� deltaTime
    public void Update(float deltaTime)
    {
        OnUpdate(deltaTime); // �����鷽�� OnUpdate
    }

    // �ܱ������鷽�����ڸ���ʱ���ã���������������д
    protected virtual void OnUpdate(float deltaTime) { }
}
