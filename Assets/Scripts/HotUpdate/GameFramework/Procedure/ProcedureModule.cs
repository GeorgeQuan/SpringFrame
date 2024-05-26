using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProcedureModule : BaseGameModule
{
    [SerializeField]
    private string[] proceduresNames = null;//程序名称
    [SerializeField]
    private string defaultProcedureName = null;//默认程序名字

    public BaseProcedure CurrentProcedure { get; private set; }//当前程序

    public bool IsRunning { get; private set; }//程序是否在运行

    public bool IsChangingProcedure {  get; private set; }//正在改变程序
    private Dictionary<Type, BaseProcedure> procedures;//程序容器,存放所有程序
    private BaseProcedure defaultProcedure;//默认程序实例
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);//请求池
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>();//请求队列

    /// <summary>
    /// 模块初始化
    /// </summary>
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        procedures = new Dictionary<Type, BaseProcedure>();
        bool findDefaultState = false;//用来判断是否查找到了默认程序
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            string procedureTypeName = proceduresNames[i];
            if (string.IsNullOrEmpty(procedureTypeName))//判断是不是空字符串
                continue;
            Type procedureType = Type.GetType(procedureTypeName,true);//通过类型名字来查找类型,true 代表找不到抛出异常,false 找不到返回null
            if (procedureType == null)//判断类型是否为空
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");//没有找到
                continue;
            }
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;//创建类型实例
            bool isDefaultState = procedureTypeName == defaultProcedureName;//判断当前程序是否是默认程序
            procedures.Add(procedureType, procedure);//添加进容器

            if (isDefaultState)//如果是默认程序
            {
                defaultProcedure = procedure;//保存实例
                findDefaultState = true;//找到了设为true
            }
        }
        if (!findDefaultState)//如果都遍历完没找到报错
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
        changeProcedureRequestPool.Clear();//清空数据,
        changeProcedureQ.Clear();
        IsRunning = false;//不在运行
    }
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }
    public async Task StartProcedure()
    {
        if (IsRunning)//如果已经运行过此方法返回
            return;
        IsRunning = true;
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//从请求池内获取请求
        changeProcedureRequest.TargetProcedure = defaultProcedure;//请求程序设置成默认程序
        changeProcedureQ.Enqueue(changeProcedureRequest);//入队
        await ChangeProcedureInternal();
    
    }
    public async Task ChangeProcedure<T>() where T : BaseProcedure
    {
        await ChangeProcedure<T>(null);
    }
    /// <summary>
    /// 改变程序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    internal async Task ChangeProcedure<T>(object value) where T : BaseProcedure
    {
        if (!IsRunning)//判断是否是运行时
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))//从容器中查找这个类型的程序
        {
          //  UnityLog.Error($"Change Procedure Failed, Can't find Proecedure:${typeof(T).FullName}");
            return;//没有就返回
        }

        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//从请求池中拿请求
        changeProcedureRequest.TargetProcedure = procedure;//把类型实例和参数给请求
        changeProcedureRequest.Value = value;
        changeProcedureQ.Enqueue(changeProcedureRequest);//请求入队

        if (!IsChangingProcedure)//判断是否在更变程序
        {
            await ChangeProcedureInternal();//没有就开始更变
        }
    }
    /// <summary>
    /// 更变程序方法
    /// </summary>
    /// <returns></returns>
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure)//判断是否更变过程序
            return;

        IsChangingProcedure = true;
        while (changeProcedureQ.Count > 0)//如果消息队列有消息
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();//消息出队
            if (request == null || request.TargetProcedure == null)//判断消息是否完整
                continue;

            if (CurrentProcedure != null)//判断当前程序是否为空
            {
                await CurrentProcedure.OnLeaveProcedure();//执行该用该程序的离开方法
            }
            CurrentProcedure = request.TargetProcedure;//切换成请求的程序
            await CurrentProcedure.OnEnterProcedure(request.Value);//执行新请求程序的进入方法,给予请求参数
        }
        IsChangingProcedure = false;//改变布尔变量状态,避免同一时间多次调用
    }
}


/// <summary>
/// 改变程序请求
/// </summary>
public class ChangeProcedureRequest
{
    public BaseProcedure TargetProcedure { get; set; }//程序
    public object Value { get; set; }//值
}
