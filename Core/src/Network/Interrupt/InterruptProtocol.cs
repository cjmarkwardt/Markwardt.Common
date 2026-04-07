namespace Markwardt;

public class InterruptProtocol(int packetSize) : IMessageProtocol<ReadOnlyMemory<byte>, InterruptPacket>
{
    public IMessageProcessor<ReadOnlyMemory<byte>, InterruptPacket> CreateProcessor()
        => new Processor(packetSize);

    private sealed class Processor(int packetSize) : MessageProcessor<ReadOnlyMemory<byte>, InterruptPacket>
    {
        private static readonly InterruptPacket receipt = new() { Type = InterruptHeader.Receipt };

        private readonly PriorityQueue<OutgoingSequence> outgoingSequences = [];
        private readonly PriorityQueue<IncomingSequence> incomingSequences = [];

        private int pendingPackets;

        protected override void SendContent(Message message, ReadOnlyMemory<byte> content)
        {
            outgoingSequences.Add(new OutgoingSequence(message, content, packetSize));
            SendPending();
        }

        protected override void ReceiveContent(Message message, InterruptPacket content)
        {
            if (content.Type is InterruptHeader.Receipt)
            {
                pendingPackets--;
                SendPending();
            }
            else
            {
                if (content.Type is InterruptHeader.Unit)
                {
                    SendReceipt();

                    message.Content = content.Data;
                    TriggerReceived(message);
                }
                else
                {
                    IncomingSequence sequence;
                    if (content.Type is InterruptHeader.Start)
                    {
                        sequence = new(content.Priority);
                        incomingSequences.Add(sequence);
                    }
                    else
                    {
                        sequence = incomingSequences.Peek(content.Priority).Value;
                    }

                    sequence.WritePacket(content.Data.Span);

                    if (content.Type is InterruptHeader.End)
                    {
                        incomingSequences.Dequeue();
                        SendReceipt();

                        message.Content = sequence.Data;
                        TriggerReceived(message);
                    }
                    else
                    {
                        SendReceipt();
                    }
                }
            }
        }

        private void SendReceipt()
            => TriggerSent(Message.New(receipt).Configure(x => x.Reliability = Reliability.Ordered));

        private void SendPending()
        {
            while (pendingPackets < 2 && outgoingSequences.Peek().TryGetValue(out OutgoingSequence? sequence))
            {
                if (sequence.IsCompleted)
                {
                    outgoingSequences.Remove(sequence);
                }
                else
                {
                    pendingPackets++;
                    TriggerSent(sequence.GetNextPacket());
                }
            }
        }

        private sealed class IncomingSequence(int priority) : IPrioritizable
        {
            private readonly MemoryBufferStream buffer = new();

            public int Priority => priority;
            public ReadOnlyMemory<byte> Data => buffer.Memory;

            public void WritePacket(ReadOnlySpan<byte> data)
                => buffer.Write(data);
        }

        private sealed class OutgoingSequence(Message message, ReadOnlyMemory<byte> content, int packetSize) : BaseDisposable, IPrioritizable
        {
            private int index = -1;

            public int Priority => message.Priority;

            public bool IsCompleted => index == content.Length;

            public Message GetNextPacket()
            {
                if (IsCompleted)
                {
                    throw new InvalidOperationException("No more packets available");
                }

                if (index < 0)
                {
                    index = 0;
                }

                int size = Math.Min(packetSize, content.Length - index);
                bool isStart = index == 0;
                bool isEnd = index + size >= content.Length;
                ReadOnlyMemory<byte> data = content.Slice(index, size);
                index += size;

                Message packet = Message.New(InterruptPacket.FromData(isStart, isEnd, Priority, data));
                packet.CopyInspects(message);
                packet.Reliability = Reliability.Ordered;

                return packet;
            }
        }
    }
}