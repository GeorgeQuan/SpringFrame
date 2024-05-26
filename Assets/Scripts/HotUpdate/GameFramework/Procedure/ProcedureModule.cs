using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProcedureModule : BaseGameModule
{
    [SerializeField]
    private string[] proceduresNames = null;//��������
    [SerializeField]
    private string defaultProcedureName = null;//Ĭ�ϳ�������

    public BaseProcedure CurrentProcedure { get; private set; }//��ǰ����

    public bool IsRunning { get; private set; }//�����Ƿ�������

    public bool IsChangingProcedure {  get; private set; }//���ڸı����
    private Dictionary<Type, BaseProcedure> procedures;//��������,������г���
    private BaseProcedure defaultProcedure;//Ĭ�ϳ���ʵ��
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);//�����
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>();//�������

    /// <summary>
    /// ģ���ʼ��
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        procedures = new Dictionary<Type, BaseProcedure>();
        bool findDefaultState = false;//�����ж��Ƿ���ҵ���Ĭ�ϳ���
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            string procedureTypeName = proceduresNames[i];
            if (string.IsNullOrEmpty(procedureTypeName))//�ж��ǲ��ǿ��ַ���
                continue;
            Type procedureType = Type.GetType(procedureTypeName,true);//ͨ��������������������,true �����Ҳ����׳��쳣,false �Ҳ�������null
            if (procedureType == null)//�ж������Ƿ�Ϊ��
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");//û���ҵ�
                continue;
            }
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;//��������ʵ��
            bool isDefaultState = procedureTypeName == defaultProcedureName;//�жϵ�ǰ�����Ƿ���Ĭ�ϳ���
            procedures.Add(procedureType, procedure);//��ӽ�����

            if (isDefaultState)//�����Ĭ�ϳ���
            {
                defaultProcedure = procedure;//����ʵ��
                findDefaultState = true;//�ҵ�����Ϊtrue
            }
        }
        if (!findDefaultState)//�����������û�ҵ�����
        {
            Debug.LogError($"You have to set a correct default procedure to start game");
        }
    }
    protected internal override void OnModuleStart()
    {
        base.OnModuleStart();
    }
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        changeProcedureRequestPool.Clear();//�������,
        changeProcedureQ.Clear();
        IsRunning = false;//��������
    }
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }
    public async Task StartProcedure()
    {
        if (IsRunning)//����Ѿ����й��˷�������
            return;
        IsRunning = true;
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//��������ڻ�ȡ����
        changeProcedureRequest.TargetProcedure = defaultProcedure;//����������ó�Ĭ�ϳ���
        changeProcedureQ.Enqueue(changeProcedureRequest);//���
        await ChangeProcedureInternal();
    
    }
    public async Task ChangeProcedure<T>() where T : BaseProcedure
    {
        await ChangeProcedure<T>(null);
    }
    /// <summary>
    /// �ı����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    internal async Task ChangeProcedure<T>(object value) where T : BaseProcedure
    {
        if (!IsRunning)//�ж��Ƿ�������ʱ
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))//�������в���������͵ĳ���
        {
          //  UnityLog.Error($"Change Procedure Failed, Can't find Proecedure:${typeof(T).FullName}");
            return;//û�оͷ���
        }

        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//���������������
        changeProcedureRequest.TargetProcedure = procedure;//������ʵ���Ͳ���������
        changeProcedureRequest.Value = value;
        changeProcedureQ.Enqueue(changeProcedureRequest);//�������

        if (!IsChangingProcedure)//�ж��Ƿ��ڸ������
        {
            await ChangeProcedureInternal();//û�оͿ�ʼ����
        }
    }
    /// <summary>
    /// ������򷽷�
    /// </summary>
    /// <returns></returns>
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure)//�ж��Ƿ���������
            return;

        IsChangingProcedure = true;
        while (changeProcedureQ.Count > 0)//�����Ϣ��������Ϣ
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();//��Ϣ����
            if (request == null || request.TargetProcedure == null)//�ж���Ϣ�Ƿ�����
                continue;

            if (CurrentProcedure != null)//�жϵ�ǰ�����Ƿ�Ϊ��
            {
                await CurrentProcedure.OnLeaveProcedure();//ִ�и��øó�����뿪����
            }
            CurrentProcedure = request.TargetProcedure;//�л�������ĳ���
            await CurrentProcedure.OnEnterProcedure(request.Value);//ִ�����������Ľ��뷽��,�����������
        }
        IsChangingProcedure = false;//�ı䲼������״̬,����ͬһʱ���ε���
    }
}


/// <summary>
/// �ı��������
/// </summary>
public class ChangeProcedureRequest
{
    public BaseProcedure TargetProcedure { get; set; }//����
    public object Value { get; set; }//ֵ
}
