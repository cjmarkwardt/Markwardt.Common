namespace Markwardt.Network;

public abstract class ConvertProcessor<TSend, TReceive> : ConnectionProcessor<TSend, TReceive>
{
    protected sealed override void SendContent(Packet packet, TSend content)
    {
        packet.Set(Convert(content));
        TriggerSent(packet);
    }

    protected sealed override void ReceiveContent(Packet packet, TReceive content)
    {
        packet.Set(Revert(content));
        TriggerReceived(packet);
    }

    protected abstract TReceive Convert(TSend content);
    protected abstract TSend Revert(TReceive content);
}