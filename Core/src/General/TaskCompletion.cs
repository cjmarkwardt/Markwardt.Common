namespace Markwardt;

public interface ITaskCompletion<out T> : ICompletion<T>
{
    void Start();
}

public class TaskCompletion<T>(Func<ValueTask<T>> action, object? source, Maybe<T> defaultResult = default) : Completion<T>(source, defaultResult), ITaskCompletion<T>
{
    private bool isStarted;

    public async void Start()
    {
        if (!isStarted)
        {
            isStarted = true;

            try
            {
                TrySetResult(await action());
            }
            catch (Exception exception)
            {
                TrySetError(exception);
            }
        }
    }
}