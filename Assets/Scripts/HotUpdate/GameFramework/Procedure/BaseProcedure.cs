using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseProcedure 
{
    /// <summary>
    /// 改变程序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task ChangeProcedure<T>(object value=null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value);
    }
    /// <summary>
    /// 进入程序
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }
    /// <summary>
    /// 离开程序
    /// </summary>
    /// <returns></returns>
    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield();
    }
}

