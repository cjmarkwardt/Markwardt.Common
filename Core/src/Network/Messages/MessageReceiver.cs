namespace Markwardt;

public interface IMessageReceiver
{
    void Receive(Message message);
}

public abstract class MessageReceiver<T> : IMessageReceiver
    where T : notnull
{
    public void Receive(Message message)
    {
        if (message.Content is T content && Filter(message, content))
        {
            Receive(message, content);
        }
    }

    protected abstract void Receive(Message message, T content);

    protected virtual bool Filter(Message message, T content)
        => true;
}