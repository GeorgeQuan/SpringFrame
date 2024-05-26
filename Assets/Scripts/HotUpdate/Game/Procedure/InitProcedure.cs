using System.Threading.Tasks;

/// <summary>
/// ≥ı ºªØ≥Ã–Ú
/// </summary>
public class InitProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }
}

