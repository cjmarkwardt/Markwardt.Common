namespace Markwardt;

public class AsyncOperation(AsyncOperationOptions options, Func<CancellationToken, ValueTask<Failable>> action)
{
    public AsyncOperation(AsyncOperationOptions options, Func<CancellationToken, ValueTask> action)
        : this(options, async cancellation => { await action(cancellation); return Failable.Success(); }) { }

    public AsyncOperation(AsyncOperationOptions options, Func<CancellationToken, Failable> action)
        : this(options, cancellation => ValueTask.FromResult(action(cancellation))) { }

    public AsyncOperation(AsyncOperationOptions options, Action<CancellationToken> action)
        : this(options, cancellation => { action(cancellation); return ValueTask.CompletedTask; }) { }
        
    public AsyncOperation(Func<CancellationToken, ValueTask<Failable>> action)
        : this(new(), action) { }

    public AsyncOperation(Func<CancellationToken, ValueTask> action)
        : this(new(), action) { }

    public AsyncOperation(Func<CancellationToken, Failable> action)
        : this(new(), action) { }

    public AsyncOperation(Action<CancellationToken> action)
        : this(new(), action) { }

    private readonly Lazy<CancellationToken[]> cancellations = new(() =>
    {
        CancellationToken[] cancellations = new CancellationToken[options.Cancellations.Count() + 1];

        int i = 0;
        foreach (CancellationToken cancellation in options.Cancellations)
        {
            cancellations[i] = cancellation;
            i++;
        }

        return cancellations;
    });

    public AsyncOperationOptions Options { get; init; } = options;
    public Func<CancellationToken, ValueTask<Failable>> Action { get; init; } = action;

    public async ValueTask<Failable> Execute(CancellationToken cancellation = default)
    {
        cancellations.Value[^1] = cancellation;
        using CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellations.Value);

        if (Options.IsThreaded)
        {
            return await Task.Run(async () => await Action(linkedCancellation.Token), linkedCancellation.Token);
        }
        else
        {
            return await Action(linkedCancellation.Token);
        }
    }
}