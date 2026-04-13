namespace Markwardt.Network;

public class ConfigureProtocol<T>(Action<Packet<T>> configure) : IConnectionProtocol<T, T>
{
    public IConnectionProcessor<T, T> CreateProcessor()
        => new Processor(configure);

    private sealed class Processor(Action<Packet<T>> configure) : ConnectionProcessor<T>
    {
        protected override void SendContent(Packet<T> packet)
            => TriggerSent(Configure(packet));

        protected override void ReceiveContent(Packet<T> packet)
            => TriggerReceived(Configure(packet));

        private Packet<T> Configure(Packet<T> packet)
        {
            configure(packet);
            return packet;
        }
    }
}