namespace Markwardt.Network;

public abstract class ConvertProcessor<TSend, TReceive> : ConnectionProcessor<TSend, TReceive>
{
    protected sealed override void SendContent(Packet<TSend> packet)
        => TriggerSent(packet.As<TReceive>().SetContent(Convert(packet.Content)));

    protected sealed override void ReceiveContent(Packet<TReceive> packet)
        => TriggerReceived(packet.As<TSend>().SetContent(Revert(packet.Content)));

    protected abstract TReceive Convert(TSend content);
    protected abstract TSend Revert(TReceive content);
}