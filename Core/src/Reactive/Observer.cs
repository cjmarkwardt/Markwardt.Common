namespace Markwardt;

public interface IObserver : IObserver<bool> { }

public abstract class Observer : IObserver
{
    public abstract void OnCompleted();
    public abstract void OnError(Exception error);
    public abstract void OnNext();

    void IObserver<bool>.OnNext(bool value)
        => OnNext();
}
