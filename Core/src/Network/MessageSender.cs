namespace Markwardt;

public interface IMessageSender
{
    void Send(Message message);
}

public interface IMessageSender<T> : IMessageSender;

public static class MessageSenderExtensions
{
    public static void Send<T>(this IMessageSender<T> sender, T content, Action<Message>? configure = null)
        => sender.Send(Message.New(content).Configure(configure));
}

public class MessageSender<T>(IMessageSender sender) : IMessageSender<T>
{
    public void Send(Message message)
        => sender.Send(message);
}