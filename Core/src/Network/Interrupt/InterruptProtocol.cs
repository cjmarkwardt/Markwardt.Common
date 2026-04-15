namespace Markwardt.Network;

public class InterruptProtocol(int packetSize) : IConnectionProtocol<ReadOnlyMemory<byte>, InterruptPacket>
{
    public IConnectionProcessor<ReadOnlyMemory<byte>, InterruptPacket> CreateProcessor()
        => new Processor(packetSize);

    private sealed class Processor(int packetSize) : ConnectionProcessor<ReadOnlyMemory<byte>, InterruptPacket>
    {
        private static readonly InterruptPacket receipt = new() { Type = InterruptHeader.Receipt };

        private readonly PriorityQueue<OutgoingSequence> outgoingSequences = [];
        private readonly PriorityQueue<IncomingSequence> incomingSequences = [];

        private int pendingPackets;

        protected override void SendContent(Packet<ReadOnlyMemory<byte>> packet)
        {
            outgoingSequences.Add(OutgoingSequence.New(packet, packetSize));
            SendPending();
        }

        protected override void ReceiveContent(Packet<InterruptPacket> packet)
        {
            InterruptHeader header = packet.Content.Type;
            if (header is InterruptHeader.Receipt)
            {
                packet.Recycle();
                pendingPackets--;
                SendPending();
            }
            else if (header is InterruptHeader.Unit)
            {
                SendReceipt();
                TriggerReceived(packet.AsContent(packet.Content.Data, packet.Recycler, false));
            }
            else
            {
                IncomingSequence sequence;
                if (header is InterruptHeader.Start)
                {
                    sequence = IncomingSequence.New(packet.Content.Priority);
                    incomingSequences.Add(sequence);
                }
                else
                {
                    sequence = incomingSequences.Peek(packet.Content.Priority).Value;
                }

                sequence.WritePacket(packet.Content.Data.Span);

                if (header is InterruptHeader.End)
                {
                    incomingSequences.Dequeue();
                    SendReceipt();
                    TriggerReceived(packet.AsContent(sequence.Data, packet.Recycler?.AppendRecycle(sequence, static sequence => sequence.Recycle()), false));
                }
                else
                {
                    packet.Recycle();
                    SendReceipt();
                }
            }
        }

        private void SendReceipt()
            => TriggerSent(Packet.New(receipt).Configure(x => x.Reliability = Reliability.Ordered));

        private void SendPending()
        {
            while (pendingPackets < 2 && outgoingSequences.Peek().TryGetValue(out OutgoingSequence? sequence))
            {
                if (sequence.IsCompleted)
                {
                    outgoingSequences.Remove(sequence);
                    sequence.Recycle();
                }
                else
                {
                    pendingPackets++;
                    TriggerSent(sequence.GetNextPacket());
                }
            }
        }

        private sealed class IncomingSequence : IPrioritizable, IRecyclable
        {
            private static readonly Pool<IncomingSequence> pool = new(() => new());

            public static IncomingSequence New(int priority)
            {
                IncomingSequence sequence = pool.Get();
                sequence.buffer = MemoryBufferStream.New();
                sequence.Priority = priority;
                return sequence;
            }

            private IncomingSequence()
                => buffer = default!;

            private MemoryBufferStream buffer;

            public int Priority { get; private set; }

            public ReadOnlyMemory<byte> Data => buffer.Memory;

            public void WritePacket(ReadOnlySpan<byte> data)
                => buffer.Write(data);

            public void Recycle()
            {
                buffer.Recycle();
                buffer = default!;

                pool.Recycle(this);
            }
        }

        private sealed class OutgoingSequence : IPrioritizable, IRecyclable
        {
            private static readonly Pool<OutgoingSequence> pool = new(() => new());

            public static OutgoingSequence New(Packet<ReadOnlyMemory<byte>> packet, int packetSize)
            {
                OutgoingSequence sequence = pool.Get();
                sequence.packet = packet;
                sequence.packetSize = packetSize;
                return sequence;
            }

            private OutgoingSequence()
                => packet = default!;

            private Packet<ReadOnlyMemory<byte>> packet;
            private int packetSize;
        
            private int index = -1;

            public int Priority => packet.Priority;

            public bool IsCompleted => index == packet.Content.Length;

            public Packet<InterruptPacket> GetNextPacket()
            {
                if (IsCompleted)
                {
                    throw new InvalidOperationException("No more packets available");
                }

                if (index < 0)
                {
                    index = 0;
                }

                int size = Math.Min(packetSize, packet.Content.Length - index);
                bool isStart = index == 0;
                bool isEnd = index + size >= packet.Content.Length;
                ReadOnlyMemory<byte> data = packet.Content.Slice(index, size);
                index += size;

                Packet<InterruptPacket> output = packet.Copy().AsContent(InterruptPacket.FromData(isStart, isEnd, Priority, data), recycle: false);
                output.Reliability = Reliability.Ordered;
                return output;
            }

            public void Recycle()
            {
                packet.Recycle();
                packet = default!;

                pool.Recycle(this);
            }
        }
    }
}