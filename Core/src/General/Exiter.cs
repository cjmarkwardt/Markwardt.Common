namespace Markwardt;

public interface IExiter
{
    void Exit();
}

public class Exiter(IServiceProvider provider) : IExiter
{
    private bool isExiting;

    public void Exit()
    {
        if (!isExiting)
        {
            isExiting = true;

            if (provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}