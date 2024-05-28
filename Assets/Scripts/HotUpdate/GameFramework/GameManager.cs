using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// ��Ϣģ��
    /// </summary>
    [Module(1)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    /// <summary>
    /// ���� ����ģ��
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get=>TGameFramework.Instance.GetModule<ProcedureModule>(); }
    /// <summary>
    /// uiģ��
    /// </summary>
    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    private bool activing;//�Ƿ������г���

    private void Awake()
    {
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        activing = true;//��ʼ��ʱ�������г���
        Application.logMessageReceived += OnReceiveLog;//��Ӵ�ӡ��־���¼�
        TGameFramework.Initialize();//���õ���
        StartupModules();
        TGameFramework.Instance.InitModules();//���ó�ʼ��ģ�鷽��
     
    }
    private void Start()
    {
        TGameFramework.Instance.StartModules();
        Procedure.StartProcedure().Coroutine();//��ʼ��Ϸ����
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
            Application.logMessageReceived -= OnReceiveLog; //�����ǰ�����־ -= ������־�ķ���
            TGameFramework.Instance.Destroy();//��TGameFrameWork ����ɾ��ʱ�����ݴ���
        }
    }
    /// <summary>
    /// ��ʼ��ģ��
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();//�����洢ʵ�������Ե�����
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);//���ұ��������з���Ҫ�������
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)//�������е�����
        {
            PropertyInfo property = propertyInfos[i];//���浱ǰ����
            if (!baseCompType.IsAssignableFrom(property.PropertyType))//�ж�������Ե������ǲ���BaseGameModule ��̳ж���
                continue;
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);//��ȡ����ModuleAttribute false ����Ѱ�Ҽ̳���
            if (attrs.Length == 0)//���������û���������,������һ��
                continue;
            Component comp = GetComponentInChildren(property.PropertyType);//�ڵ�ǰ��Ϸ�����²�������Ϊ�������͵����
            if (comp == null)//˵�����ģ��û�б������ڶ�����
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;//�õ�����
            moduleAttr.Module = comp as BaseGameModule;//��������ģ��
            moduleAttrs.Add(moduleAttr);//��ӽ�����
        }
        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });//������ģ������ȼ���������
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }


    }



    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]//��������
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>//�̳�����������,�̳бȽϽӿ�
    {
        /// <summary>
        /// ģ��������ȼ�
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// ģ��
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
    /// ��ӡ��־�ķ��� #if ��ֻ��Ӧ����ִ��,����unity ��ִ��
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
