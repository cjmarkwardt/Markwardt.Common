namespace Markwardt;

public static class ChannelProtocolExtensions
{
    public static IChannelManager? GetChannelManager(this IMessageSender sender)
        => MessageInterceptor.GetInterceptors(sender).OfType<IChannelManager>().FirstOrDefault();

    public static IMessageChannel<T> OpenChannel<T>(this IMessageSender<T> sender, TimeSpan? autoAssertDelay)
        => sender.GetChannelManager()?.OpenChannel(autoAssertDelay).As<T>() ?? throw new InvalidOperationException("Sender does not support channels");

    public static IMessageChannelValue<T> OpenChannelValue<T, TContent>(this IMessageSender<TContent> sender, TimeSpan sendInterval, T value, Func<T, TContent> write, TimeSpan? autoAssertDelay)
        => new MessageChannelValue<T, TContent>(sender.OpenChannel(autoAssertDelay), sendInterval, value, write);
}

public class ChannelProtocol<T>(IValueWindow? sequenceWindow = null) : IMessageProtocol<T, T>
    where T : IChannelPacket
{
    private readonly IValueWindow sequenceWindow = sequenceWindow ?? new ValueWindow(4000);

    public IMessageProcessor<T, T> CreateProcessor()
        => new Processor(this);

    private sealed class Processor(ChannelProtocol<T> protocol) : ValueHeaderProcessor<T, MessageChannelHeader>
    {
        protected override InspectValueKey<MessageChannelHeader> ValueHeaderKey => ChannelHeaderKey.Instance;

        protected override Maybe<MessageChannelHeader> GetValueHeader(T content)
            => content.GetChannel();

        protected override void SetValueHeader(T content, MessageChannelHeader header)
            => content.SetChannel(header);

        protected override IMessageInterceptor? CreateInterceptor()
            => new Interceptor(protocol);

        private sealed class Interceptor : MessageInterceptor, IChannelManager
        {
            public Interceptor(ChannelProtocol<T> protocol)
            {
                this.protocol = protocol;
                this.RunInBackground(AutoAssert);
            }

            private readonly ChannelProtocol<T> protocol;
            private readonly IdSet channelIds = new(1);
            private readonly Dictionary<int, int> sentSequences = [];
            private readonly Dictionary<int, int> receivedSequences = [];
            private readonly Dictionary<int, Channel> channels = [];

            public IEnumerable<IMessageChannel> Channels => channels.Values;

            public IMessageChannel OpenChannel(TimeSpan? autoAssertDelay)
            {
                Channel channel = new(this, channelIds.Next(), autoAssertDelay);
                channels.Add(channel.Id, channel);
                return channel;
            }

            protected override IEnumerable<Message>? Intercept(Message message)
            {
                if (message.Inspect(ChannelHeaderKey.Instance).TryGetValue(out MessageChannelHeader header))
                {
                    if (protocol.sequenceWindow.IsNext(receivedSequences.TryGetValue(header.Channel, out int sequence) ? sequence : null, header.Sequence))
                    {
                        receivedSequences[header.Channel] = header.Sequence;
                    }
                    else
                    {
                        return [];
                    }
                }

                return null;
            }

            private void Send(int channel, bool assert, Message message)
            {
                int sequence = protocol.sequenceWindow.Next(sentSequences.TryGetValue(channel, out int sentSequence) ? sentSequence : null);
                sentSequences[channel] = sequence;

                if (assert)
                {
                    channels.GetValueOrDefault(channel)?.SetAsserted();
                }
                else
                {
                    message = message.Copy();
                    message.Recycler = null;
                }

                message.Reliability = assert ? Reliability.Reliable : Reliability.Unreliable;
                message.SetInspect(ChannelHeaderKey.Instance, new MessageChannelHeader(channel, sequence));

                Sender.Send(message);
            }

            private async ValueTask AutoAssert(CancellationToken cancellation)
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(250, cancellation);
                    channels.Values.Where(x => x.NeedsAutoAssert).ForEach(x => x.Asserter.Send());
                }
            }

            private sealed class Channel : BaseDisposable, IMessageChannel
            {
                public Channel(Interceptor interceptor, int id, TimeSpan? autoAssertDelay)
                {
                    this.interceptor = interceptor;
                    this.id = id;
                    AutoAssertDelay = autoAssertDelay;
                    Asserter = new MessageAsserter(this);
                }

                private readonly Interceptor interceptor;
                private readonly int id;

                private Message? pendingMessage;
                private bool isFirstSend = true;
                private DateTime lastAssert;

                public bool IsPending => pendingMessage is not null;

                public TimeSpan? AutoAssertDelay { get; set; }

                public int Id => id;
                public bool NeedsAutoAssert => IsPending && AutoAssertDelay.HasValue && DateTime.UtcNow >= lastAssert + AutoAssertDelay.Value;

                public IMessageAsserter Asserter { get; }

                public void SetAsserted()
                {
                    lastAssert = DateTime.UtcNow;
                    pendingMessage = null;
                }

                public void Send(Message message)
                {
                    if (isFirstSend)
                    {
                        isFirstSend = false;
                        Asserter.Send(message);
                    }
                    else
                    {
                        pendingMessage?.Recycle();
                        pendingMessage = message;

                        interceptor.Send(id, false, message);
                    }
                }

                protected override void OnDispose()
                {
                    base.OnDispose();

                    interceptor.channelIds.Release(id);
                    interceptor.channels.Remove(id);
                }

                private sealed class MessageAsserter(Channel channel) : IMessageAsserter
                {
                    public void Send(Message message)
                        => channel.interceptor.Send(channel.id, true, message);

                    public void Send()
                    {
                        if (channel.pendingMessage is not null)
                        {
                            Send(channel.pendingMessage);
                        }
                    }
                }
            }
        }
    }
}

/*public class ChannelProtocol<T>() : HeaderProtocol<T, MessageChannelHeader?>(MessageParameters.ChannelHeader)
{
    protected override IMessageInterceptor? CreateInterceptor()
        => new Interceptor();

    private sealed class Interceptor : MessageInterceptor, IChannelManager
    {
        public Interceptor()
            => this.RunInBackground(AutoAssert);

        private readonly IdSet channelIds = new(1);
        private readonly Dictionary<int, Channel> channels = [];
        private readonly Dictionary<int, int> receivedSequences = [];

        public IEnumerable<IMessageChannel> Channels => channels.Values;

        public IMessageChannel OpenChannel(TimeSpan? autoAssertDelay)
        {
            Channel channel = new(this, channelIds.Next(), autoAssertDelay);
            channels.Add(channel.Id, channel);
            return channel;
        }

        protected override IEnumerable<Message>? Intercept(Message message)
        {
            if (message.GetParameter(MessageParameters.ChannelHeader) is MessageChannelHeader header)
            {
                if (header.Sequence == 0 || header.Sequence > receivedSequences.GetValueOrDefault(header.Channel))
                {
                    receivedSequences[header.Channel] = header.Sequence;
                }
                else
                {
                    return [];
                }
            }

            return null;
        }

        private async ValueTask AutoAssert(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(250, cancellation);
                channels.Values.Where(x => x.NeedsAutoAssert).ForEach(x => x.Assert());
            }
        }

        private sealed class Channel(Interceptor interceptor, int id, TimeSpan? autoAssertDelay) : BaseDisposable, IMessageChannel
        {
            private Message? pendingMessage;
            private bool isFirstSend = true;
            private DateTime lastAssert;
            private int sequence;

            private MessageChannelHeader Header => new(id, sequence);

            public bool IsPending => pendingMessage is not null;

            public TimeSpan? AutoAssertDelay { get; set; } = autoAssertDelay;

            public int Id => id;
            public bool NeedsAutoAssert => IsPending && AutoAssertDelay.HasValue && DateTime.UtcNow >= lastAssert + AutoAssertDelay.Value;

            public void Assert()
            {
                if (pendingMessage is not null)
                {
                    lastAssert = DateTime.UtcNow;
                    sequence = 0;
                    Message assertMessage = pendingMessage.SetParameter(MessageParameters.ChannelHeader, Header).SetParameter(MessageParameters.Reliability, Reliability.Reliable);
                    pendingMessage = null;
                    interceptor.Sender.Send(assertMessage);
                }
            }

            public void Send(Message message)
            {
                pendingMessage?.Recycle();
                pendingMessage = message;

                if (isFirstSend)
                {
                    isFirstSend = false;
                    Assert();
                }
                else
                {
                    sequence++;
                    interceptor.Sender.Send(message.Copy().SetRecycler(null).SetParameter(MessageParameters.ChannelHeader, Header).SetParameter(MessageParameters.Reliability, Reliability.Unreliable));
                }
            }

            protected override void OnDispose()
            {
                base.OnDispose();

                interceptor.channelIds.Release(id);
                interceptor.channels.Remove(id);
            }
        }
    }
}

/*public static class ChannelProtocolExtensions
{
    private static ChannelProtocol.IInterceptor GetChannelInterceptor<T>(INetworkConnection<T> connection)
        => NetworkInterceptor.GetInterceptors(connection).OfType<ChannelProtocol.IInterceptor>().FirstOrDefault() ?? throw new InvalidOperationException("Connection must have a channel interceptor to support channels");

    public static INetworkChannel<T> OpenChannel<T>(this INetworkConnection<T> connection, TimeSpan? autoAssertDelay)
        => GetChannelInterceptor(connection).OpenChannel(autoAssertDelay).As<T>();

    public static IEnumerable<INetworkChannel<T>> GetChannels<T>(this INetworkConnection<T> connection)
        => GetChannelInterceptor(connection).Channels.Select(x => x.As<T>());
}

public class ChannelProtocol : INetworkProtocol<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public INetworkProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor();

    internal interface INetworkChannel
    {
        INetworkChannel<T> As<T>();
    }

    internal interface IInterceptor
    {
        IEnumerable<INetworkChannel> Channels { get; }

        INetworkChannel OpenChannel(TimeSpan? autoAssertDelay);
    }

    private sealed class Processor : NetworkProcessor<ReadOnlyMemory<byte>>
    {
        private readonly Interceptor interceptor = new();

        protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Append(interceptor);

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            interceptor.Dispose();
        }
        
        protected override void SendContent(Message message, ReadOnlyMemory<byte> content)
        {
            NetworkChannelHeader? header = message.GetParameter(NetworkParameters.ChannelId);
            int channel = header?.Channel ?? 0;
            int channelLength = VariableInteger.GetLength(channel);
            int sequence = header?.Sequence ?? 0;
            int sequenceLength = header is null ? 0 : VariableInteger.GetLength(sequence);
            Memory<byte> data = new byte[channelLength + sequenceLength + content.Length];

            int index = 0;
            index += VariableInteger.Write(data.Span[index..], channel);

            if (header is not null)
            {
                index += VariableInteger.Write(data.Span[index..], sequence);
            }

            content.CopyTo(data[index..]);

            TriggerSentContent(data, x => x.Parameters.Overwrite(message.Parameters));
        }

        protected override void ReceiveContent(Message message, ReadOnlyMemory<byte> content)
        {
            int index = 0;

            int channel = (int)VariableInteger.Read(content.Span[index..], out int channelLength);
            index += channelLength;

            int sequenceLength = 0;
            int sequence = channel == 0 ? 0 : (int)VariableInteger.Read(content.Span[index..], out sequenceLength);
            index += sequenceLength;

            content = content[index..];

            TriggerReceivedContent(content, x =>
            {
                x.Parameters.Overwrite(message.Parameters);

                if (channel > 0)
                {
                    x.SetParameter(NetworkParameters.ChannelId, new(channel, sequence));
                }
            });
        }
    }

    private sealed class Interceptor : NetworkInterceptor, IInterceptor
    {
        public Interceptor()
            => this.RunInBackground(AutoAssert);

        private readonly IdSet channelIds = new(1);
        private readonly Dictionary<int, Channel> channels = [];
        private readonly Dictionary<int, int> receivedSequences = [];

        public IEnumerable<INetworkChannel> Channels => channels.Values;

        public INetworkChannel OpenChannel(TimeSpan? autoAssertDelay)
        {
            Channel channel = new(this, channelIds.Next(), autoAssertDelay);
            channels.Add(channel.Id, channel);
            return channel;
        }

        protected override bool Intercept(Message message)
        {
            if (message.GetParameter(NetworkParameters.ChannelId) is NetworkChannelHeader header)
            {
                if (header.Sequence == 0 || header.Sequence > receivedSequences.GetValueOrDefault(header.Channel))
                {
                    receivedSequences[header.Channel] = header.Sequence;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private async ValueTask AutoAssert(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(250, cancellation);
                channels.Values.Where(x => x.NeedsAutoAssert).ForEach(x => x.Assert());
            }
        }

        private sealed class Channel(Interceptor interceptor, int id, TimeSpan? autoAssertDelay) : BaseDisposable, INetworkChannel
        {
            private Message? pendingMessage;
            private bool isFirstSend = true;
            private DateTime lastAssert;
            private int sequence;

            private bool IsPending => pendingMessage is not null;
            private NetworkChannelHeader Header => new(id, sequence);

            public TimeSpan? AutoAssertDelay { get; set; } = autoAssertDelay;

            public int Id => id;
            public bool NeedsAutoAssert => IsPending && AutoAssertDelay.HasValue && DateTime.UtcNow >= lastAssert + AutoAssertDelay.Value;

            public INetworkChannel<T> As<T>()
                => new TypedChannel<T>(this);

            public void Assert()
            {
                if (pendingMessage is not null)
                {
                    lastAssert = DateTime.UtcNow;
                    sequence = 0;
                    Message assertMessage = pendingMessage.SetParameter(NetworkParameters.ChannelId, Header).SetParameter(NetworkParameters.Reliability, NetworkReliability.Reliable);
                    pendingMessage = null;
                    interceptor.Send(assertMessage);
                }
            }

            private void Send(Message message)
            {
                pendingMessage?.Recycle();
                pendingMessage = message;

                if (isFirstSend)
                {
                    isFirstSend = false;
                    Assert();
                }
                else
                {
                    sequence++;
                    interceptor.Send(message.Copy().SetRecycler(null).SetParameter(NetworkParameters.ChannelId, Header).SetParameter(NetworkParameters.Reliability, NetworkReliability.Unreliable));
                }
            }

            protected override void OnDispose()
            {
                base.OnDispose();

                interceptor.channelIds.Release(id);
                interceptor.channels.Remove(id);
            }

            private sealed class TypedChannel<T>(Channel channel) : BaseDisposable, INetworkChannel<T>
            {
                public bool IsPending => channel.IsPending;

                public TimeSpan? AutoAssertDelay { get => channel.AutoAssertDelay; set => channel.AutoAssertDelay = value; }

                public void Send(Message message)
                    => channel.Send(message);

                public void Assert()
                    => channel.Assert();

                protected override void OnDispose()
                {
                    base.OnDispose();

                    channel.Dispose();
                }
            }
        }
    }
}*/