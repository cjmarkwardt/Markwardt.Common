namespace Markwardt;

public class TcpBlockLink(TcpClient client, int clampedCapacity, (string Host, int Port)? connectTarget = null) : BaseDisposable, INetworkLink
{
    private readonly Buffer<byte> lengthBuffer = new();
    private readonly Buffer<byte> readBuffer = new();

    protected TcpClient Client => client;

    public INetworkLink.IHandler? Handler { get; set; }

    public async ValueTask Run(CancellationToken cancellation = default)
        => await Task.WhenAll(RunRead(cancellation), RunMonitor(cancellation));

    public async ValueTask Connect(CancellationToken cancellation = default)
    {
        if (connectTarget is null)
        {
            throw new InvalidOperationException("No connect target specified.");
        }

        await client.ConnectAsync(connectTarget.Value.Host, connectTarget.Value.Port, cancellation);
    }

    public async ValueTask Send(ReadOnlyMemory<byte> data, NetworkReliability mode, CancellationToken cancellation = default)
    {
        BigInteger length = data.Length;
        byte lengthSize = (byte)length.GetByteCount(true);

        lengthBuffer.Resize(1 + lengthSize);
        lengthBuffer.Span[0] = lengthSize;
        length.TryWriteBytes(lengthBuffer.Span[1..], out _, true);

        await client.GetStream().WriteAsync(lengthBuffer.Memory, cancellation);
        await client.GetStream().WriteAsync(data, cancellation);
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        client.Dispose();
    }

    private async Task RunRead(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            await Read(cancellation);
        }
    }

    private async Task RunMonitor(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellation);

            if (!TestConnected())
            {
                Handler?.OnDropped(new NetworkException("Connection lost"));
            }
        }
    }

    private async ValueTask Read(CancellationToken cancellation = default)
    {
        if (!client.Connected)
        {
            await Task.Delay(50, cancellation);
            return;
        }

        NetworkStream stream = client.GetStream();

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

            if (Handler is not null)
            {
                Handler.OnReceived(readBuffer.Memory);
            }
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