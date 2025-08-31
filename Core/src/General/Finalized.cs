namespace Markwardt;

public abstract class Finalized : IDisposable
{
    ~Finalized()
        => ExecuteDispose();

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        ExecuteDispose();
        GC.SuppressFinalize(this);
    }

    protected abstract void Release();

    private void ExecuteDispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;
            Release();
        }
    }
}

public abstract class Finalized<T>(T value) : Finalized
{
    public T Value
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return value;
        }
    }

    protected sealed override void Release()
        => Release(value);

    protected abstract void Release(T value);
}