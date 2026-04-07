namespace Markwardt;

public class TransformedSender(IMessageSender sender, Func<Message, Message> transform) : IMessageSender
{
    public void Send(Message message)
        => sender.Send(transform(message));
}