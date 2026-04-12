namespace Markwardt.Network;

public class RateLimitProtocol(int rate) : IConnectionProtocol<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(rate);

    private sealed class Processor(int rate) : ConnectionProcessor<ReadOnlyMemory<byte>>
    {
        private readonly Queue<QueuedMessage> sendQueue = [];

        private bool isWriting;

        protected override async void SendContent(Packet<ReadOnlyMemory<byte>> packet)
        {
            sendQueue.Enqueue(new QueuedMessage(packet, packet.Content.Length));

            if (!isWriting)
            {
                isWriting = true;

                while (sendQueue.TryDequeue(out QueuedMessage nextMessage))
                {
                    try
                    {
                        await TimeSpan.FromSeconds((float)nextMessage.Length / rate).Delay(Disposal);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    TriggerSent(nextMessage.Message);
                }

                isWriting = false;
            }
        }

        private record struct QueuedMessage(Packet Message, int Length);
    }
}