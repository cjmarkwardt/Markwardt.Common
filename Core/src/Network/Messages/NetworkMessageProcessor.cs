namespace Markwardt;

public interface INetworkMessageProcessor : INetworkChannelManager
{
    INetworkMessageProcessorHandler? Handler { get; set; }

    void Send(object message, NetworkConstraints constraints = NetworkConstraints.All);
    ValueTask<object> Request(object message, TimeSpan? timeout = null, CancellationToken cancellation = default);
    void SafetySend();
    void Receive(ReadOnlySpan<byte> data);
}

public class NetworkMessageProcessor : INetworkMessageProcessor
{
    public NetworkMessageProcessor(INetworkConnection connection, INetworkMessageConnection messageConnection, INetworkMessageSerializer serializer)
    {
        this.connection = connection;
        this.messageConnection = messageConnection;
        this.serializer = serializer;

        Channels = channels.ConvertValues(x => (INetworkChannel)x);
    }

    private readonly INetworkConnection connection;
    private readonly INetworkMessageConnection messageConnection;
    private readonly INetworkMessageSerializer serializer;
    private readonly HashSet<Channel> unsafeChannels = [];
    private readonly HashSet<Channel> safeChannelBuffer = [];
    private readonly IdRange channelIds = new();
    private readonly IdRange requestIds = new();
    private readonly Dictionary<int, TaskCompletionSource<object>> requests = [];

    public INetworkMessageProcessorHandler? Handler { get; set; }

    private readonly Dictionary<int, Channel> channels = [];
    public IReadOnlyDictionary<int, INetworkChannel> Channels { get; }

    public INetworkChannel CreateChannel(int id)
    {
        Channel channel = new(this, id);
        channels.Add(id, channel);
        return channel;
    }

    public void Send(object message, NetworkConstraints constraints = NetworkConstraints.All)
        => connection.Send(buffer =>
        {
            buffer.WriteVariableInteger(0, (VariableIntegerOption)Header.Message, true);
            serializer.Serialize(messageConnection, null, message, buffer);
            Handler?.OnRecycled(message);
        }, constraints);

    public async ValueTask<object> Request(object message, TimeSpan? timeout = null, CancellationToken cancellation = default)
    {
        timeout ??= TimeSpan.FromSeconds(3);

        int id = requestIds.Next();
        TaskCompletionSource<object> completion = new();
        requests.Add(id, completion);
        
        connection.Send(buffer =>
        {
            buffer.WriteVariableInteger(id, (VariableIntegerOption)Header.Request, true);
            serializer.Serialize(messageConnection, null, message, buffer);
            Handler?.OnRecycled(message);
        });

        try
        {
            return await completion.Task.WithTimeout(timeout, cancellation);
        }
        finally
        {
            requestIds.Release(id);
            requests.Remove(id);
        }
    }

    public void SafetySend()
    {
        foreach (Channel channel in unsafeChannels)
        {
            if (channel.TrySafetySend())
            {
                safeChannelBuffer.Add(channel);
            }
        }

        if (safeChannelBuffer.Count > 0)
        {
            foreach (Channel channel in safeChannelBuffer)
            {
                unsafeChannels.Remove(channel);
            }

            safeChannelBuffer.Clear();
        }
    }

    public void Receive(ReadOnlySpan<byte> data)
    {
        int index = 0;
        int value = (int)data.ReadVariableInteger(index, out index, out VariableIntegerOption option, true);
        Header action = (Header)option;

        object ReadMessage(Channel? channel, ReadOnlySpan<byte> data)
            => serializer.Deserialize(messageConnection, channel, data[index..]);

        if (action is Header.Message)
        {
            if (value == 0)
            {
                Handler?.OnReceived(ReadMessage(null, data), null);
            }
            else if (channels.TryGetValue(value, out Channel? channel))
            {
                byte sequence = data.Read(index, out index);
                if (channel.Receive(sequence))
                {
                    Handler?.OnReceived(ReadMessage(channel, data), channel);
                }
            }
        }
        else if (action is Header.Request)
        {
            HandleRequest(value, ReadMessage(null, data));
        }
        else if ((action is Header.Response || action is Header.Rejection) && requests.TryGetValue(value, out TaskCompletionSource<object>? request))
        {
            if (action is Header.Response)
            {
                request.TrySetResult(ReadMessage(null, data));
            }
            else if (action is Header.Rejection)
            {
                request.TrySetException(new NetworkRequestRejectedException(data.ReadString(index, out index, out _)));
            }
        }
    }

    private async void HandleRequest(int requestId, object request)
    {
        string? rejection = null;
        object? response = null;

        if (Handler is null)
        {
            rejection = "Network requests are not handled";
        }
        else
        {
            try
            {
                response = await Handler.OnRequested(request);
            }
            catch (NetworkRequestRejectedException exception)
            {
                rejection = exception.Message;
            }
        }

        connection.Send(buffer =>
        {
            if (response is not null)
            {
                buffer.WriteVariableInteger(requestId, (VariableIntegerOption)Header.Response, true);
                serializer.Serialize(messageConnection, null, response, buffer);
                Handler?.OnRecycled(response);
            }
            else
            {
                buffer.WriteVariableInteger(requestId, (VariableIntegerOption)Header.Rejection, true);
                buffer.WriteString(rejection.NotNull());
            }
        });
    }

    private enum Header
    {
        Message = 0,
        Request = 0b1,
        Response = 0b10,
        Rejection = 0b11
    }

    private class Channel(NetworkMessageProcessor processor, int id) : INetworkChannel
    {
        private readonly Buffer<byte> lastSendData = new();

        private bool hasSent;
        private DateTime lastSend;
        private byte lastSentSequence;
        private byte lastReceivedSequence;

        public int Id => id;

        public object? Profile { get; set; }
        public TimeSpan SafetyDelay { get; set; }

        public void Send(object message)
        {
            processor.unsafeChannels.Add(this);
            hasSent = true;
            lastSend = DateTime.UtcNow;
            lastSentSequence++;

            processor.connection.Send(buffer =>
            {
                buffer.WriteVariableInteger(Id, (VariableIntegerOption)Header.Message, true);
                buffer.Write(lastSentSequence);
                processor.serializer.Serialize(processor.messageConnection, this, message, buffer);
                processor.Handler?.OnRecycled(message);
                lastSendData.Fill(buffer.Span);
            }, NetworkConstraints.None);
        }

        public bool TrySafetySend()
        {
            if (!hasSent)
            {
                return true;
            }
            else if (DateTime.UtcNow > lastSend + SafetyDelay)
            {
                processor.connection.Send(buffer => buffer.Fill(lastSendData.Span), NetworkConstraints.Reliable);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Receive(byte sequence)
        {
            if (lastReceivedSequence < sequence)
            {
                lastReceivedSequence = sequence;
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            processor.channels.Remove(id);
            processor.channelIds.Release(id);
        }
    }
}