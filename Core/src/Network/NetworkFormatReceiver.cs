namespace Markwardt;

public interface INetworkFormatReceiver
{
    ValueTask Receive(INetworkFormatHandler handler, INetworkEncryptor? encryptor, ReadOnlyMemory<byte> data);
}

public class NetworkFormatReceiver(INetworkSerializer serializer) : INetworkFormatReceiver
{
    private readonly MemoryReader<byte> receiveReader = new();
    private readonly MemoryWriteStream decryptionStream = new();

    public async ValueTask Receive(INetworkFormatHandler handler, INetworkEncryptor? encryptor, ReadOnlyMemory<byte> data)
    {
        receiveReader.Position = 0;

        object ReadMessage()
            => serializer.Deserialize(receiveReader, data.Span);

        int id = (int)receiveReader.ReadVariableInteger(data.Span, out int embeddedValue);
        bool isEncrypted = embeddedValue.GetBit(2);
        NetworkHeader header = (NetworkHeader)embeddedValue.ClearBit(2);

        if (isEncrypted)
        {
            if (encryptor is null)
            {
                throw new NetworkException("Received encrypted message over insecure connection.");
            }

            encryptor.Decrypt(receiveReader.ReadRemaining(data.Span), decryptionStream);
            data = decryptionStream.Buffer.Memory;
            receiveReader.Position = 0;
        }

        try
        {
            switch (header)
            {
                case NetworkHeader.Send:

                    if (id == 0)
                    {
                        await handler.OnMessage(ReadMessage());
                    }
                    else if (id < 0)
                    {
                        await handler.OnResponse(-id, ReadMessage());
                    }
                    else
                    {
                        await handler.OnRequest(id, ReadMessage());
                    }

                    break;

                case NetworkHeader.Update:

                    await handler.OnUpdate(id, receiveReader.Read(data.Span), ReadMessage());

                    break;

                case NetworkHeader.Sync:

                    await handler.OnSync(id, receiveReader.Read(data.Span));

                    break;

                case NetworkHeader.Control:

                    await handler.OnControl(NetworkControlMessage.Read((NetworkControlHeader)id, serializer, receiveReader, data.Span));

                    break;
            }
        }
        finally
        {
            decryptionStream.SetLength(0);
        }
    }
}