namespace Markwardt.Network;

public class ConfigureProtocol<T>(Action<T, Packet> configure) : IConnectionProtocol<T, T>
{
    public IConnectionProcessor<T, T> CreateProcessor()
        => new Processor(configure);

    private sealed class Processor(Action<T, Packet> configure) : ConnectionProcessor<T>
    {
        protected override void SendContent(Packet packet, T content)
            => TriggerSent(Configure(packet, content));

        protected override void ReceiveContent(Packet packet, T content)
            => TriggerReceived(Configure(packet, content));

        private Packet Configure(Packet packet, T content)
        {
            configure(content, packet);
            return packet;
        }
    }
}