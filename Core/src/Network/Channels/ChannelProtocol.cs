namespace Markwardt.Network;

public static class ChannelProtocolExtensions
{
    public static IChannelManager? GetChannelManager(this ISender sender)
        => NetworkInterceptor.GetInterceptors(sender).OfType<IChannelManager>().FirstOrDefault();

    public static IObservable<(Packet Message, T Content, IObservable<(Packet Message, T Content)> Messages)> GetReceivedChannels<T>(this ISender<T> sender)
        => sender.GetChannelManager()?.Received.Select(x => (x.Message, (T)x.Message.Content!, x.Messages.Select(y => (y, (T)y.Content!)))) ?? throw new InvalidOperationException("Sender does not support channels");

    public static IChannel<T> OpenChannel<T>(this ISender<T> sender, T packet, TimeSpan? autoAssertDelay, Action<Packet>? configureMessage = null)
        => sender.GetChannelManager()?.OpenChannel(Packet.New(packet).Configure(configureMessage), autoAssertDelay).As<T>() ?? throw new InvalidOperationException("Sender does not support channels");

    public static IChannelValue<T> OpenChannelValue<T, TContent>(this ISender<TContent> sender, TimeSpan sendInterval, TContent content, T initialValue, Func<T, TContent> write, TimeSpan? autoAssertDelay)
        => new ChannelValue<T, TContent>(sender.OpenChannel(content, autoAssertDelay), sendInterval, initialValue, write);
}

public class ChannelProtocol<T>(IValueWindow? sequenceWindow = null) : IConnectionProtocol<T, T>
    where T : IHeaderPacket<ChannelHeader>, IConstructable<T>
{
    public IConnectionProcessor<T, T> CreateProcessor()
        => new Processor(sequenceWindow);

    private sealed class Processor : HeaderrProcessor<T, ChannelHeader>
    {
        public Processor(IValueWindow? sequenceWindow)
        {
            interceptor = new(this);
            this.sequenceWindow = sequenceWindow ?? new ValueWindow(4000);
        }

        private readonly Interceptor interceptor;
        private readonly IValueWindow sequenceWindow;

        protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Concat([interceptor]);

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            interceptor.Dispose();
        }

        private sealed class Interceptor : NetworkInterceptor, IChannelManager
        {
            public Interceptor(Processor processor)
            {
                this.processor = processor;
                this.RunInBackground(AutoAssert);
            }

            private readonly Processor processor;
            private readonly IdSet channelIds = new();
            private readonly Dictionary<int, OutputChannel> outputs = [];
            private readonly Dictionary<int, InputChannel> inputs = [];

            public IEnumerable<IChannel> Channels => outputs.Values;

            private readonly BufferSubject<(Packet Message, IObservable<Packet> Messages)> received = new();
            public IObservable<(Packet Message, IObservable<Packet> Messages)> Received => received;

            public IChannel OpenChannel(Packet packet, TimeSpan? autoAssertDelay)
            {
                OutputChannel channel = new(this, channelIds.Next(), autoAssertDelay);
                outputs.Add(channel.Id, channel);
                channel.SendOpen(channel.Id, packet);
                return channel;
            }

            protected override IEnumerable<Packet>? Intercept(Packet packet)
            {
                if (processor.GetHeader(packet).TryGetValue(out ChannelHeader header))
                {
                    if (inputs.TryGetValue(header.Channel, out InputChannel? input))
                    {
                        if (header.Part is ChannelPart.Close)
                        {
                            input.Dispose();
                            inputs.Remove(header.Channel);
                        }
                        else if (header.Part is ChannelPart.Data)
                        {
                            input.Receive(header.Sequence, packet);
                        }
                    }
                    else if (header.Part is ChannelPart.Open)
                    {
                        input = new(this);
                        inputs.Add(header.Channel, input);
                        received.OnNext((packet, input.Messages));
                    }

                    return [];
                }

                return null;
            }

            private async ValueTask AutoAssert(CancellationToken cancellation)
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(250, cancellation);
                    outputs.Values.Where(x => x.NeedsAutoAssert).ForEach(x => x.Assert());
                }
            }

            private sealed class InputChannel(Interceptor interceptor) : BaseDisposable
            {
                private int? sequence;

                private readonly BufferSubject<Packet> received = new();
                public IObservable<Packet> Messages => received;

                public void Receive(int sequence, Packet packet)
                {
                    if (interceptor.processor.sequenceWindow.IsNext(this.sequence, sequence))
                    {
                        this.sequence = sequence;
                        received.OnNext(packet);
                    }
                }

                protected override void OnDispose()
                {
                    base.OnDispose();

                    received.Dispose();
                }
            }

            private sealed class OutputChannel : BaseDisposable, IChannel
            {
                public OutputChannel(Interceptor interceptor, int id, TimeSpan? autoAssertDelay)
                {
                    this.interceptor = interceptor;
                    this.id = id;
                    AutoAssertDelay = autoAssertDelay;
                    Asserter = new Sender(packet => SendAssert(id, packet));
                }

                private readonly Interceptor interceptor;
                private readonly int id;

                private int? sequence;
                private Packet? pendingMessage;
                private DateTime lastAssert;

                public bool IsPending => pendingMessage is not null;

                public TimeSpan? AutoAssertDelay { get; set; }

                public int Id => id;
                public bool NeedsAutoAssert => IsPending && AutoAssertDelay.HasValue && DateTime.UtcNow >= lastAssert + AutoAssertDelay.Value;

                public ISender Asserter { get; }

                ISender IChannel.Asserter => Asserter;

                public void Assert()
                {
                    if (pendingMessage is not null)
                    {
                        Asserter.Send(pendingMessage);
                    }
                }

                public void SendOpen(int channel, Packet packet)
                    => Send(channel, ChannelPart.Open, Reliability.Ordered, packet);

                public void SendData(int channel, Packet packet)
                {
                    packet = packet.Copy();
                    packet.Recycler = null;
                    Send(channel, ChannelPart.Data, Reliability.Unreliable, packet);
                }

                public void SendAssert(int channel, Packet packet)
                {
                    lastAssert = DateTime.UtcNow;
                    pendingMessage = null;
                    Send(channel, ChannelPart.Data, Reliability.Reliable, packet);
                }

                public void SendClose(int channel)
                    => Send(channel, ChannelPart.Close, Reliability.Ordered, Packet.New(T.New()));

                void ISender.Send(Packet packet)
                {
                    pendingMessage?.Recycle();
                    pendingMessage = packet;

                    SendData(id, packet);
                }

                protected override void OnDispose()
                {
                    base.OnDispose();
                    
                    interceptor.channelIds.Release(id);
                    interceptor.outputs.Remove(id);

                    SendClose(id);
                }

                private void Send(int channel, ChannelPart part, Reliability reliability, Packet packet)
                {
                    sequence = interceptor.processor.sequenceWindow.Next(sequence);

                    packet.Reliability = reliability;
                    interceptor.processor.SetHeader(packet, new(channel, part, sequence.Value));
                    interceptor.Sender.Send(packet);
                }
            }
        }
    }
}