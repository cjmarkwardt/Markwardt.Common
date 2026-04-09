namespace Markwardt;

public interface IRequestManager
{
    ValueTask<Message> Request(Message message, TimeSpan? timeout, CancellationToken cancellation);
}