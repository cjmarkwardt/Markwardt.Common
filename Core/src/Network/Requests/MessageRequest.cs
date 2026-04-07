namespace Markwardt;

public interface IMessageRequest : IMessageSender
{
    Message Message { get; }
}

public interface IMessageRequest<T> : IMessageRequest, IMessageSender<T>
{
    T Content { get; }
}

public static class MessageRequestExtensions
{
    public static IMessageRequest<T> As<T>(this IMessageRequest request)
        => new MessageRequest<T>(request);
}

public class MessageRequest(Message message, Action<Message> respond) : IMessageRequest
{
    public Message Message => message;

    public void Send(Message message)
        => respond(message);
}

public class MessageRequest<T>(IMessageRequest request) : IMessageRequest<T>
{
    public Message Message => request.Message;
    public T Content => (T)request.Message.Content!;

    public void Send(Message message)
        => request.Send(message);
}