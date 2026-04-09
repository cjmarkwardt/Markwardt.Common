namespace Markwardt;

public static class SteamExtensions
{
    public static IHost<TSend> HostSteam<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, int port)
        => protocol.Host(new SteamHoster(port));

    public static IConnection<TSend> ConnectSteam<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, SteamTarget target, int port)
        => protocol.Connect(new SteamConnector(target, port));

    internal static async ValueTask<Failable<T>> Callback<T>(Func<T, bool> filter, CancellationToken cancellation = default)
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

    internal static async ValueTask<Failable<T>> Consume<T>(this SteamAPICall_t call, CancellationToken cancellation = default)
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