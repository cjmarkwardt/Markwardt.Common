namespace Markwardt;

public interface IDisposable<out T> : ITrackedDisposable
{
    T Value { get; }
}

public class Disposable<T>(T value, Action dispose) : BaseDisposable, IDisposable<T>
{
    public T Value
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return value;
        }
    }

    protected override void OnDispose()
        => dispose();
}