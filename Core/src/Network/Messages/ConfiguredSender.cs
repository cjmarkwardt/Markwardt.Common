namespace Markwardt.Network;

public interface IConfiguredSender : ISender
{
    void Configure(ISender sender);
}

public class ConfiguredSender : IConfiguredSender
{
    private ISender? sender;
    private ISender Sender => sender ?? throw new InvalidOperationException("Message sender not configured.");

    public void Configure(ISender sender)
        => this.sender = sender;

    public void Send(Packet packet)
        => Sender.Send(packet);
}