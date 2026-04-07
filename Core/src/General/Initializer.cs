namespace Markwardt;

public interface IInitializer
{
    void Initialize();
}

public abstract class Initializer : IInitializer
{
    private bool isInitialized;

    public void Initialize()
    {
        if (!isInitialized)
        {
            isInitialized = true;
            OnInitialize();
        }
    }

    protected abstract void OnInitialize();
}