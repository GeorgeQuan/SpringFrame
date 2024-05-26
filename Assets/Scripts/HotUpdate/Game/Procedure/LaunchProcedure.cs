
 
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 启动程序
/// </summary>
public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
      
        await LoadConfigs();
        await ChangeProcedure<InitProcedure>();
    }

    private async Task LoadConfigs()
    {
       
        await Task.Yield();
     


    
    }
}

