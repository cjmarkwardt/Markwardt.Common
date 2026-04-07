namespace Markwardt;

/// <summary> Can capture received network messages instead of them being put into the receive queue. </summary>
public interface IMessageInterceptor
{
    void Attach(IMessageSender sender);

    /// <returns> Messages that will replace the intercepted message, or null if no interception occurred. </returns>
    IEnumerable<Message>? Intercept(IMessageSender sender, Message message);
}

public abstract class MessageInterceptor : BaseDisposable, IMessageInterceptor
{
    public static IEnumerable<IMessageInterceptor> GetInterceptors(IMessageSender sender)
        => (sender as IMessageInterceptable)?.Interceptors ?? [];

    private IMessageSender? sender;
    protected IMessageSender Sender => sender ?? throw new InvalidOperationException("Interceptor is not attached to a sender");

    public void Attach(IMessageSender sender)
        => this.sender = sender;

    public IEnumerable<Message>? Intercept(IMessageSender sender, Message message)
        => this.sender == sender ? Intercept(message) : null;

    protected abstract IEnumerable<Message>? Intercept(Message message);
}