using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TGameFramework
{
    public static TGameFramework Instance { get; private set; }

    public static bool Initialized { get; private set; }

    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();//�������ģ��

    /// <summary>
    /// ��ʼ�� ���ڵ���
    /// </summary>
    public static void Initialize()
    {
        Instance = new TGameFramework();
    }

    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))//Ѱ��ģ���оͷ���
        {
            return module as T;
        }
        return default(T);//�������͵�Ĭ��ֵ,�ڷ��ͷ�����ʹ��
    }
    /// <summary>
    /// ��ʼ��ģ��
    /// </summary>
    public void InitModules()
    {
        if (Initialized)//���ģ���Ѿ���ʼ�����˷���
            return;

        Initialized = true;
        foreach (var module in m_modules.Values)//�������е�ģ��
        {
            module.OnModuleInit();//����ģ��ĳ�ʼ������
        }
    }
    /// <summary>
    /// ���ģ��
    /// </summary>
    /// <param name="module"></param>
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
           // UnityLog.Info($"Module���ʧ���ظ�:{moduleType}");
            return;
        }
        m_modules.Add(moduleType, module);//����ģ������
    }


    internal void StartModules()
    {
        if (m_modules == null)//������ģ��������ǿ�,����
            return;
        if (!Initialized)//���û�����÷���
            return;
        foreach (var module in m_modules.Values)//��������ģ��
        {
            module.OnModuleStart();//����ģ��Start����
        }

    }

    internal void Update()
    {
        if (!Initialized)//���û�г�ʼ��������
            return;
        if (m_modules == null)//û��ģ�鷵��
            return;

        if (!Initialized)//�ٴ��ж�
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleUpdate(deltaTime);//�������е�Updata����
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
            module.OnModuleStop();//��������ģ���ֹͣ����
        }

        //Destroy(Instance.gameObject);
        Instance = null;//��յ���ʵ��
        Initialized = false;//��ʼ������Ϊδ��ʼ��
    }
}
