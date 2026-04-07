namespace Markwardt;

public interface IMessageRequester
{
    ValueTask<Message> Request(Message message, TimeSpan? timeout, CancellationToken cancellation);
}