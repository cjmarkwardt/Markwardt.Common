namespace Markwardt;

public class RateLimitProtocol(int rate) : IMessageProtocol<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public IMessageProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(rate);

    private sealed class Processor(int rate) : MessageProcessor<ReadOnlyMemory<byte>>
    {
        private readonly Queue<QueuedMessage> sendQueue = [];

        private bool isWriting;

        protected override async void SendContent(Message message, ReadOnlyMemory<byte> content)
        {
            sendQueue.Enqueue(new QueuedMessage(message, content.Length));

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

        private record struct QueuedMessage(Message Message, int Length);
    }
}