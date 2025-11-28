namespace Markwardt;

public abstract record NetworkControlMessage(NetworkControlHeader Header)
{
    public static NetworkControlMessage Read(NetworkControlHeader header, INetworkSerializer serializer, MemoryReader<byte> reader, ReadOnlySpan<byte> data)
    {
        object? ReadMessage(ReadOnlySpan<byte> data)
            => reader.ReadBoolean(data) ? serializer.Deserialize(reader, data) : null;

        return header switch
        {
            NetworkControlHeader.Connect => new NetworkConnectMessage(ReadMessage(data)),
            NetworkControlHeader.CompleteConnect => new NetworkCompleteConnectMessage(ReadMessage(data)),
            NetworkControlHeader.Secure => new NetworkSecureMessage(),
            NetworkControlHeader.CreateSession => new NetworkCreateSessionMessage(reader.ReadBlock(data)),
            NetworkControlHeader.StartSession => new NetworkStartSessionMessage(reader.ReadBlock(data)),
            NetworkControlHeader.Register => new NetworkRegisterMessage(reader.ReadString(data), reader.ReadBlock(data), ReadMessage(data)),
            NetworkControlHeader.Authenticate => new NetworkAuthenticateMessage(reader.ReadString(data)),
            NetworkControlHeader.Disconnect => new NetworkDisconnectMessage(reader.ReadString(data)),
            NetworkControlHeader.OpenChannel => new NetworkOpenChannelMessage((int)reader.ReadVariableInteger(data), ReadMessage(data).NotNull()),
            NetworkControlHeader.CloseChannel => new NetworkCloseChannelMessage((int)reader.ReadVariableInteger(data)),
            _ => throw new InvalidOperationException($"Unknown control header: {header}"),
        };
    }

    public abstract void Write(INetworkSerializer serializer, IBuffer<byte> buffer);

    protected void WriteMessage(INetworkSerializer serializer, IBuffer<byte> buffer, object? message)
    {
        buffer.WriteBoolean(message is not null);
        if (message is not null)
        {
            serializer.Serialize(message, buffer);
        }
    }
}