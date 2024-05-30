using Config;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UIģ��
/// </summary>

public partial class UIModule : BaseGameModule
{
    public Transform normalUIRoot;//����UI��
    public Transform modalUIRoot;//ģ��UI��
    public Transform closeUIRoot;//�ر�UI��
    public Image imgMask;//ͼƬ����
    public QuantumConsole prefabQuantumConsole;//���ӿ�����û�õ�

    private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING;//UIID �н�������
    private static Dictionary<UIViewID, Type> ASSET_MAPPING;//UIID  View����

    private readonly List<UIMediator> usingMediators = new List<UIMediator>();//�򿪵��н�������
    private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();//�н��߳�
    private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();//ui�����
    private QuantumConsole quantumConsole;//���ӿ���̨

    /// <summary>
    /// ��ʼ��û�õ�
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
    /// ֹͣû�õ�
    /// </summary>
    protected internal override void OnModuleStop()
    {
        //base.OnModuleStop();
        //quantumConsole.OnActivate -= OnConsoleActive;
        //quantumConsole.OnDeactivate -= OnConsoleDeactive;
    }
    /// <summary>
    /// ����uiӳ�� ,����ʾUIʱ�ȵ���
    /// </summary>
    private static void CacheUIMapping()
    {
        if (MEDIATOR_MAPPING != null)//����н��������Ѿ���Ϊ����,��ζ���Ѿ�ִ�й���,return
            return;

        MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();//��ʼ��
        ASSET_MAPPING = new Dictionary<UIViewID, Type>();

        Type baseViewType = typeof(UIView);//�õ����ͷ���
        foreach (var type in baseViewType.Assembly.GetTypes())//��ȡ�������ڵĳ��������е�����
        {
            if (type.IsAbstract)//����ǳ�����ͷ���
                continue;

            if (baseViewType.IsAssignableFrom(type))//�ж�type �ǲ���baseViewType ��������
            {
                object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);//��ȡ������ָ������,����������
                if (attrs.Length == 0)//���Ϊ��,û������,Ҳ����Ϊ��û�а��н���
                {
                    UnityLog.Error($"{type.FullName} û�а� Mediator����ʹ��UIMediatorAttribute��һ��Mediator����ȷʹ��");
                    continue;
                }

                foreach (UIViewAttribute attr in attrs)//������������
                {
                    MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);//IDΪ��,�洢�н�������
                    ASSET_MAPPING.Add(attr.ID, type);//IDΪ��,�洢View����
                    break;
                }
            }
        }
    }
    /// <summary>
    /// upDate����
    /// </summary>
    /// <param name="deltaTime"></param>

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        uiObjectPool.UpdateLoadRequests();//Update�����������,������ж��Ƿ�������
        foreach (var mediator in usingMediators)//�����н�������
        {
            mediator.Update(deltaTime);//�����н��ߵ�UPdate
        }
        UpdateMask(deltaTime);//��������
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
    /// ��ȡ��ǰģʽ����ߵ���Ⱦ�㼶
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private int GetTopMediatorSortingOrder(UIMode mode)
    {
        int lastIndexMediatorOfMode = -1;
        for (int i = usingMediators.Count - 1; i >= 0; i--)//������������н�������
        {
            UIMediator mediator = usingMediators[i];//��ȡ�н��� ʵ��
            if (mediator.UIMode != mode)//�ж�uimode �Ƿ�ƥ��
                continue;

            lastIndexMediatorOfMode = i;//ƥ���ϱ����±�
            break;
        }

        if (lastIndexMediatorOfMode == -1)//-1 �Ļ���ζ��û�����ģʽ�������е��н���
            return mode == UIMode.Normal ? 0 : 1000;//�ж�ģʽ�ǲ���normal ���ز�ֵͬ

        return usingMediators[lastIndexMediatorOfMode].SortingOrder;//���������������ģʽ���н���ʵ������Ⱦ�㼶
    }
    /// <summary>
    /// ��ȡ�н���
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private UIMediator GetMediator(UIViewID id)
    {
        CacheUIMapping();//��Ѱ���н���

        if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))//����id �����н�������
        {
            UnityLog.Error($"�Ҳ��� {id} ��Ӧ��Mediator");
            return null;
        }

        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//ͨ���н������Ͳ���(ʵ��)
        {
            mediatorQ = new Queue<UIMediator>();//���û�оʹ����н��߶���
            freeMediators.Add(mediatorType, mediatorQ);//��Ž��ֵ�
        }

        UIMediator mediator;//������������
        if (mediatorQ.Count == 0)//�������������Ϊ��
        {
            mediator = Activator.CreateInstance(mediatorType) as UIMediator;//�������ʹ���ʵ�� ����
        }
        else
        {
            mediator = mediatorQ.Dequeue();//�����ֱ�ӳ��� ����
        }

        return mediator;
    }
    /// <summary>
    /// �����н���
    /// </summary>
    /// <param name="mediator"></param>
    private void RecycleMediator(UIMediator mediator)//�����н��߶���
    {
        if (mediator == null)//�п�
            return;

        Type mediatorType = mediator.GetType();//��ȡ����
        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//���н��߳��в����Ƿ�Ϊ��
        {
            mediatorQ = new Queue<UIMediator>();//Ϊ�վʹ������
            freeMediators.Add(mediatorType, mediatorQ);//��ӽ�����
        }
        mediatorQ.Enqueue(mediator);//���
    }
    /// <summary>
    /// ��ȡ�򿪵�ui�н���
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UIMediator GetOpeningUIMediator(UIViewID id)//����ID
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//�����ñ��ڲ���
        if (uiConfig.IsNull)//������ñ���û�з���
            return null;

        UIMediator mediator = GetMediator(id);//����ui��ȡ�н��߶���
        if (mediator == null)
            return null;

        Type requiredMediatorType = mediator.GetType();//�õ���������
        foreach (var item in usingMediators)//�����н�������
        {
            if (item.GetType() == requiredMediatorType)//�ҵ������е��н��� ����
                return item;
        }
        return null;
    }
    /// <summary>
    /// ʹ�ö�
    /// </summary>
    /// <param name="id"></param>
    public void BringToTop(UIViewID id)//����uiID
    {
        UIMediator mediator = GetOpeningUIMediator(id);//�Ӵ򿪵��н��������ڲ����н���ʵ��
        if (mediator == null)//�п�
            return;

        int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);//��ȡ��ģʽ�����ϲ����Ⱦ�㼶
        if (mediator.SortingOrder == topSortingOrder)//����Ѿ�����ߵ� ֱ�ӷ���
            return;

        int sortingOrder = topSortingOrder + 10;//�ڵ�ǰ��߲㼶�Ļ�����������10,��Ϊ��߲��
        mediator.SortingOrder = sortingOrder;//��ֵ��������ֵ

        usingMediators.Remove(mediator);//ɾ���н���

        usingMediators.Add(mediator);//������ӽ�ĩβ

        Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();//��ȡCanvas
        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder;//�޸�Canvas �㼶 Ҳ��������
        }
    }
    /// <summary>
    /// �ж�ui�Ƿ����
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsUIOpened(UIViewID id)
    {
        return GetOpeningUIMediator(id) != null;//����Ѿ��򿪵��н�����û�л᷵��null
    }
    /// <summary>
    /// ʹ�ö�����
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
    /// ��ui
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public UIMediator OpenUI(UIViewID id, object arg = null)//����uiID
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//�ж��Ƿ�������
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);//�����н���ʵ��
        if (mediator == null)
            return null;
        //������Դ
        GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
        {
            //��ȡ���
            UIView newView = obj.GetComponent<UIView>();
            //�н��߳�ʼ��
            mediator.InitMediator(newView);
        })).gameObject;
        return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
    }
    /// <summary>
    /// �첽��UI (����)ֻ�ܴ�һ��
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
    {
        if (!IsUIOpened(id))//�ж��Ƿ��Ѿ�����
        {
            yield return OpenUIAsync(id, arg);//û�򿪵��ô򿪷���
        }
    }
    /// <summary>
    /// �첽��UI
    /// </summary>
    /// <param name="id"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public IEnumerator OpenUIAsync(UIViewID id, object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);//�����ñ��в���
        if (uiConfig.IsNull)
            yield break;

        UIMediator mediator = GetMediator(id);//���н��߶���
        if (mediator == null)
            yield break;

        bool loadFinish = false;//�����Ƿ����bool
        //�����첽������Ϸ���󷽷�
        uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
        {
            GameObject uiObject = asset.gameObject;
            OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
            loadFinish = true;//�������
        }, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();//��ȡ��UIView
            mediator.InitMediator(newView);//��ʼ���н���
        });
        while (!loadFinish)
        {
            yield return null;
        }
        yield return null;
        yield return null;
        
    }
    /// <summary>
    /// UI������غ���
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="uiConfig"></param>
    /// <param name="uiObject"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
    {
        if (uiObject == null)//�ж�ui�����Ƿ�Ϊ��
        {
            UnityLog.Error($"����UIʧ��:{uiConfig.Asset}");
            RecycleMediator(mediator);//�����н���
            return null;
        }

        UIView view = uiObject.GetComponent<UIView>();//��ȡui����View���
        if (view == null)//�ж��Ƿ�Ϊ��
        {
            UnityLog.Error($"UI Prefab������UIView�ű�:{uiConfig.Asset}");
            RecycleMediator(mediator);//�����н���
            uiObjectPool.UnloadGameObject(view.gameObject);//��������Ϸ����,
            return null;
        }

        mediator.UIMode = uiConfig.Mode;//���н��߸���mode
        int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;//���¼���㼶

        usingMediators.Add(mediator);//��ӽ�����

        Canvas canvas = uiObject.GetComponent<Canvas>();//��ȡ�����Canvax
        canvas.renderMode = RenderMode.ScreenSpaceCamera;//�޸�Canvasģʽ
        //canvas.worldCamera = GameManager.Camera.uiCamera;
        //���ݲ�ͬ��ģʽ��Ӳ�ͬ�ĸ�
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

        mediator.SortingOrder = sortingOrder;//��ֵ�µĲ㼶
        canvas.sortingOrder = sortingOrder;

        uiObject.SetActive(true);//������Ϸ����
        mediator.Show(uiObject, obj);//������ʾ����
        return mediator;//�����н��߶���
    }
    /// <summary>
    /// �ر�UI
    /// </summary>
    /// <param name="mediator"></param>
    public void CloseUI(UIMediator mediator)
    {
        if (mediator != null)
        {
            // ����View
            uiObjectPool.UnloadGameObject(mediator.ViewObject);
            mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

            // ����Mediator
            mediator.Hide();
            RecycleMediator(mediator);

            usingMediators.Remove(mediator);//��������ɾ���н��߶���
        }
    }
    /// <summary>
    /// �ر�����UI
    /// </summary>
    public void CloseAllUI()
    {
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            CloseUI(usingMediators[i]);
        }
    }
    /// <summary>
    /// �ر�ָ��UI
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
    /// ��������������͵�UI�Ƿ�ɼ�
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
    /// ��������
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
    //Ŀ��͸����
    private float destMaskAlpha = 0;
    //���ֳ���ʱ��
    private float maskDuration = 0;
    private void UpdateMask(float deltaTime)
    {
        Color c = imgMask.color;//��ȡ����Image ��ɫ
        //�������ʱ��û�н���
        //��ǰ͸����,Ŀ��͸����,��ֵ  �������ֱ�Ӹ�ֵĿ��͸����
        c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
        c.a = Mathf.Clamp01(c.a);//Լ����0-1
        imgMask.color = c;//����������ɫ
        imgMask.enabled = imgMask.color.a > 0;//�����ȫ͸����������
    }

    public void ShowConsole()
    {
        quantumConsole.Activate();//�������ӿ���̨
    }
}

/// <summary>
/// �Զ�������,�����½���ʱ����UI��id ���н�������
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
