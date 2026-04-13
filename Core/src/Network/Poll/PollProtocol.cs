namespace Markwardt.Network;

public class PollProtocol<T>(TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null) : IConnectionProtocol<T, T>
    where T : IPollPacket, IConstructable<T>
{
    public IConnectionProcessor<T, T> CreateProcessor()
        => new Processor(pollInterval ?? TimeSpan.FromSeconds(1), pollTimeout ?? (pollInterval ?? TimeSpan.FromSeconds(1)) * 3);

    private sealed class Processor(TimeSpan pollInterval, TimeSpan pollTimeout) : ConnectionProcessor<T>
    {
        private DateTime lastReceived;

        protected override void OnConnected()
        {
            base.OnConnected();

            lastReceived = DateTime.UtcNow;

            poll = this.RunInBackground(RunPoll);
        }

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            poll?.Dispose();
        }

        private IDisposable? poll;

        protected override void ReceiveContent(Packet<T> packet)
        {
            lastReceived = DateTime.UtcNow;

            if (!packet.Content.IsPoll())
            {
                TriggerReceived(packet);
            }
        }

        private async ValueTask RunPoll(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await pollInterval.Delay(cancellation);

                if (DateTime.UtcNow > lastReceived.Add(pollTimeout))
                {
                    TriggerDisconnect(new TimeoutException($"Poll not received in time"));
                    return;
                }
                else
                {
                    TriggerSent(Packet.New(T.New()));
                }
            }
        }
    }
}