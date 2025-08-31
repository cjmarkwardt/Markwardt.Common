namespace Markwardt;

public sealed class TcpBlockConnection(INetworkConnectionController controller, TcpClient client, int clampedCapacity) : BaseDisposable
{
    private readonly Buffer<byte> lengthBuffer = new();
    private readonly Buffer<byte> readBuffer = new();
    private readonly NetworkStream stream = client.GetStream();

    public async ValueTask Connect(string host, int port, CancellationToken cancellation)
        => await client.ConnectAsync(host, port, cancellation);

    public async ValueTask Run(CancellationToken cancellation)
        => await Task.WhenAll(RunRead(controller, cancellation), RunMonitor(cancellation));

    public async ValueTask Send(ReadOnlyMemory<byte> data, CancellationToken cancellation)
    {
        BigInteger length = data.Length;
        byte lengthSize = (byte)length.GetByteCount(true);

        lengthBuffer.Resize(1 + lengthSize);
        lengthBuffer.Span[0] = lengthSize;
        length.TryWriteBytes(lengthBuffer.Span[1..], out _, true);

        await stream.WriteAsync(lengthBuffer.Memory, cancellation);
        await stream.WriteAsync(data, cancellation);
    }

    public ValueTask Close(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected override void OnDispose()
    {
        base.OnDispose();
        client.Dispose();
    }

    private async Task RunRead(INetworkConnectionController connection, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            await Read(connection.Receive, cancellation);
        }
    }

    private async Task RunMonitor(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellation);

            if (!TestConnected())
            {
                throw new NetworkException("Connection lost");
            }
        }
    }

    private async ValueTask Read(MemoryConsumer<byte> receive, CancellationToken cancellation = default)
    {
        readBuffer.Resize(1);
        await stream.ReadExactlyAsync(readBuffer.Memory, cancellation);

        int lengthSize = readBuffer.Span[0];
        if (lengthSize > 0)
        {
            readBuffer.Resize(lengthSize);
            await stream.ReadExactlyAsync(readBuffer.Memory, cancellation);
            BigInteger length = new(readBuffer.Span, true);

            readBuffer.Resize((int)length);
            await stream.ReadExactlyAsync(readBuffer.Memory, cancellation);
            receive(readBuffer.Span);
        }

        readBuffer.ClampCapacity(clampedCapacity);
    }

    private bool TestConnected()
    {
        try
        {
            Socket socket = client.Client;
            if (socket.Connected)
            {
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    Span<byte> buffer = stackalloc byte[1];
                    if (socket.Receive(buffer, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}