namespace Markwardt;

public class PollProtocol<T>(TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null) : IMessageProtocol<T, T>
    where T : IPollPacket, IConstructable<T>
{
    public IMessageProcessor<T, T> CreateProcessor()
        => new Processor(pollInterval ?? TimeSpan.FromSeconds(1), pollTimeout ?? (pollInterval ?? TimeSpan.FromSeconds(1)) * 3);

    private sealed class Processor(TimeSpan pollInterval, TimeSpan pollTimeout) : MessageProcessor<T>
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

        protected override void ReceiveContent(Message message, T content)
        {
            lastReceived = DateTime.UtcNow;

            if (!content.IsPoll())
            {
                TriggerReceived(message);
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
                    TriggerSent(Message.New(T.New()));
                }
            }
        }
    }
}