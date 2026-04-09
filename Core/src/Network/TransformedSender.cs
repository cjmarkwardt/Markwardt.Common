namespace Markwardt.Network;

public class TransformedSender(ISender sender, Func<Packet, Packet> transform) : ISender
{
    public void Send(Packet packet)
        => sender.Send(transform(packet));
}