namespace Markwardt;

public interface IRequest
{
    int RequestId { get; }

    ValueTask<Message> GetResponse(TimeSpan? timeout = null, CancellationToken cancellation = default);
}