namespace Markwardt;

public class ConfigureProtocol<T>(Action<T, Message> configure) : IMessageProtocol<T, T>
{
    public IMessageProcessor<T, T> CreateProcessor()
        => new Processor(configure);

    private sealed class Processor(Action<T, Message> configure) : MessageProcessor<T>
    {
        protected override void SendContent(Message message, T content)
            => TriggerSent(Configure(message, content));

        protected override void ReceiveContent(Message message, T content)
            => TriggerReceived(Configure(message, content));

        private Message Configure(Message message, T content)
        {
            configure(content, message);
            return message;
        }
    }
}