namespace Markwardt;

public class Observer<T>(Action<T>? onNext = null, Action? onCompleted = null, Action<Exception>? onError = null) : IObserver<T>
{
    public void OnNext(T value)
        => onNext?.Invoke(value);

    public void OnCompleted()
        => onCompleted?.Invoke();

    public void OnError(Exception error)
        => onError?.Invoke(error);
}