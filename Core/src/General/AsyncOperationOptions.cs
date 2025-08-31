namespace Markwardt;

public record AsyncOperationOptions
{
    public bool IsThreaded { get; init; } = false;
    public IEnumerable<CancellationToken> Cancellations { get; init; } = [];
}