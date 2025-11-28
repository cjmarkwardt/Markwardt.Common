namespace Markwardt;

public interface INetworkFormatSender
{
    INetworkBlockPool? DefaultPool { get; set; }

    void SendMessage(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, NetworkReliability reliability, object message);
    void SendRequest(INetworkRemoteConnection connection, INetworkBlockPool? pool, NetworkSecurity? security, int requestId, object message);
    void SendResponse(INetworkRemoteConnection connection, INetworkBlockPool? pool, NetworkSecurity? security, int requestId, object message);
    void SendUpdate(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, int channelId, object? channelProfile, byte sequence, object message);
    void SendSync(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, int channelId, byte sequence);
    void SendControl(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, NetworkControlMessage message);
}

public class NetworkFormatSender(INetworkSerializer serializer, bool secureByDefault) : INetworkFormatSender
{
    private readonly NetworkSendQueue sendQueue = new();

    public INetworkBlockPool? DefaultPool { get => sendQueue.DefaultPool; set => sendQueue.DefaultPool = value; }

    public void SendMessage(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, NetworkReliability reliability, object message)
        => Send(connections, pool, security, reliability, NetworkHeader.Send, 0, buffer => serializer.Serialize(message, buffer));

    public void SendRequest(INetworkRemoteConnection connection, INetworkBlockPool? pool, NetworkSecurity? security, int requestId, object message)
        => Send(connection.Yield(), pool, security, NetworkReliability.Reliable, NetworkHeader.Send, requestId, buffer => serializer.Serialize(message, buffer));

    public void SendResponse(INetworkRemoteConnection connection, INetworkBlockPool? pool, NetworkSecurity? security, int requestId, object message)
        => Send(connection.Yield(), pool, security, NetworkReliability.Reliable, NetworkHeader.Send, -requestId, buffer => serializer.Serialize(message, buffer));

    public void SendUpdate(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, int channelId, object? channelProfile, byte sequence, object message)
        => Send(connections, pool, security, NetworkReliability.Unreliable, NetworkHeader.Update, channelId, buffer =>
        {
            buffer.Write(sequence);
            serializer.Serialize(message, buffer);
        });

    public void SendSync(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, int channelId, byte sequence)
        => Send(connections, pool, security, NetworkReliability.Unreliable, NetworkHeader.Sync, channelId, buffer => buffer.Write(sequence));

    public void SendControl(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, NetworkControlMessage message)
        => Send(connections, pool, security, NetworkReliability.Ordered, NetworkHeader.Control, (int)message.Header, buffer => message.Write(serializer, buffer));

    private void Send(IEnumerable<INetworkRemoteConnection> connections, INetworkBlockPool? pool, NetworkSecurity? security, NetworkReliability reliability, NetworkHeader header, BigInteger id, Action<IBuffer<byte>> write)
    {
        void Push(IEnumerable<INetworkRemoteConnection> connections, INetworkEncryptor? encryptor)
            => sendQueue.Enqueue(connections.Select(x => x.Link), reliability, encryptor, pool, buffer => buffer.WriteVariableInteger(id, ((int)header).SetBit(2, encryptor is not null)), write);

        security ??= (secureByDefault ? NetworkSecurity.Secure : NetworkSecurity.Insecure);

        switch (security)
        {
            case NetworkSecurity.Insecure:

                Push(connections, null);

                break;

            case NetworkSecurity.TrySecure:

                Push(connections.Where(x => x.Encryptor is null), null);

                foreach (INetworkRemoteConnection connection in connections.Where(x => x.Encryptor is not null))
                {
                    Push(connection.Yield(), connection.Encryptor);
                }

                break;

            case NetworkSecurity.Secure:

                foreach (INetworkRemoteConnection connection in connections)
                {
                    if (connection.Encryptor is null)
                    {
                        throw new InvalidOperationException("Cannot send secure message over insecure connection.");
                    }

                    Push(connection.Yield(), connection.Encryptor);
                }

                break;
        }
    }
}