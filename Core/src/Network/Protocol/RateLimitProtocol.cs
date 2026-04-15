namespace Markwardt.Network;

public class RateLimitProtocol(int rate) : IConnectionProtocol<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(rate);

    private sealed class Processor(int rate) : ConnectionProcessor<ReadOnlyMemory<byte>>
    {
        private readonly Queue<Packet<ReadOnlyMemory<byte>>> sendQueue = [];

        private bool isWriting;

        protected override async void SendContent(Packet<ReadOnlyMemory<byte>> packet)
        {
            sendQueue.Enqueue(packet);

            if (!isWriting)
            {
                isWriting = true;

                while (sendQueue.TryDequeue(out Packet<ReadOnlyMemory<byte>> nextMessage))
                {
                    try
                    {
                        await TimeSpan.FromSeconds((float)nextMessage.Content.Length / rate).Delay(Disposal);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    TriggerSent(nextMessage);
                }

                isWriting = false;
            }
        }
    }
}