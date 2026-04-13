namespace Markwardt.Network;

public interface IHeaderPacket<THeader>
{
    Maybe<THeader> GetHeader();
    void SetHeader(THeader header);
}

public abstract class HeaderProcessor<T, THeader> : ConnectionProcessor<T>
    where T : IHeaderPacket<THeader>
    where THeader : struct
{
    private readonly InspectValueKey<THeader> headerKey = new(typeof(THeader).Name);

    protected Maybe<THeader> GetHeader(Packet packet)
        => packet.Inspect(headerKey);

    protected void SetHeader(Packet packet, THeader header)
        => packet.SetInspect(headerKey, header);

    protected override void SendContent(Packet<T> packet)
    {
        if (packet.Inner.Inspect(headerKey).TryGetValue(out THeader header))
        {
            packet.Content.SetHeader(header);
        }

        TriggerSent(packet);
    }

    protected override void ReceiveContent(Packet<T> packet)
    {
        if (packet.Content.GetHeader().TryGetValue(out THeader header))
        {
            packet.Inner.SetInspect(headerKey, header);
        }

        TriggerReceived(packet);
    }
}