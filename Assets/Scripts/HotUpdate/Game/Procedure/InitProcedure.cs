using System.Threading.Tasks;

/// <summary>
/// ��ʼ������
/// </summary>
public class InitProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }
}

