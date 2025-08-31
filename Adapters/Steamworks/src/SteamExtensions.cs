namespace Markwardt;

public static class SteamExtensions
{
    public static async ValueTask<Failable<T>> Callback<T>(Func<T, bool> filter, CancellationToken cancellation = default)
    {
        TaskCompletionSource<Failable<T>> completion = new();

        using Callback<T> listener = Steamworks.Callback<T>.Create(x =>
        {
            if (filter(x))
            {
                completion.SetResult(x);
            }
        });

        using IDisposable subscription = cancellation.Register(completion.SetCanceled);
        
        try
        {
            return await completion.Task;
        }
        catch (OperationCanceledException)
        {
            return Failable.Cancel<T>();
        }
    }

    public static async ValueTask<Failable<T>> Consume<T>(this SteamAPICall_t call, CancellationToken cancellation = default)
    {
        TaskCompletionSource<Failable<T>> completion = new();
        using CallResult<T> listener = CallResult<T>.Create((result, isFailed) => completion.SetResult(isFailed ? Failable.Fail<T>("Steam call failed") : result));
        listener.Set(call);
        using IDisposable subscription = cancellation.Register(completion.SetCanceled);
        
        try
        {
            return await completion.Task;
        }
        catch (OperationCanceledException)
        {
            return Failable.Cancel<T>();
        }
    }
}