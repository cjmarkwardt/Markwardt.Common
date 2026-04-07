namespace Markwardt;

public interface IConfiguredMessageSender : IMessageSender
{
    void Configure(IMessageSender sender);
}

public class ConfiguredMessageSender : IConfiguredMessageSender
{
    private IMessageSender? sender;
    private IMessageSender Sender => sender ?? throw new InvalidOperationException("Message sender not configured.");

    public void Configure(IMessageSender sender)
        => this.sender = sender;

    public void Send(Message message)
        => Sender.Send(message);
}