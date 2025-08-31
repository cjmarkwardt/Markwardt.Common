namespace Markwardt;

public interface ICompletion<out T> : IObservable<T>
{
    object? Source { get; }
    bool IsComplete { get; }
    Exception? Error { get; }
    IMaybe<T> Result { get; }
}

public class Completion<T>(object? source = null, Maybe<T> defaultResult = default) : ICompletion<T>
{
    private readonly List<IObserver<T>> observers = [];

    public Exception? Error { get; private set; }
    public bool IsComplete { get; private set; }
    public IMaybe<T> Result { get; private set; } = defaultResult;

    public object? Source => source;

    public void TrySetResult(T result)
        => TryComplete(() => Result = result.Maybe());

    public void TrySetError(Exception error)
        => TryComplete(() => Error = error);

    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (IsComplete)
        {
            CompleteObserver(observer);
            return Disposable.Empty;
        }
        else
        {
            observers.Add(observer);
            return Disposable.Create(() => observers.Remove(observer));
        }
    }

    private void CompleteObserver(IObserver<T> observer)
    {
        if (Error is not null)
        {
            observer.OnError(Error);
        }
        else
        {
            observer.OnNext(Result.Value);
        }

        observer.OnCompleted();
    }

    private void TryComplete(Action complete)
    {
        if (!IsComplete)
        {
            IsComplete = true;
            complete();

            foreach (IObserver<T> observer in observers)
            {
                CompleteObserver(observer);
            }
        }
    }
}