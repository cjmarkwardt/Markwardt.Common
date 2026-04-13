namespace Markwardt.Network;

public class IpConnectionKey : InspectKey<IIpConnectionInfo>
{
    public static IpConnectionKey Instance { get; } = new();
    
    private IpConnectionKey()
        : base(nameof(IpConnectionKey)) { }
}

public interface IIpConnectionInfo
{
    bool IsOutgoing { get; }
    bool IsIncoming { get; }
    string Host { get; }
    int Port { get; }
}

internal class TcpConnection : Connection<ReadOnlyMemory<byte>>
{
    public TcpConnection(TcpClient client, (string, int)? target, MemoryPool<byte>? pool)
    {
        this.client = client;
        this.target = target;
        this.pool = pool;
        Inspectable.SetInspect(IpConnectionKey.Instance, new Info(this));

        run = this.RunInBackground(Run);
    }

    private readonly TcpClient client;
    private readonly (string Host, int Port)? target;
    private readonly MemoryPool<byte>? pool;
    private readonly IDisposable run;
    private readonly BufferBlock<Packet<ReadOnlyMemory<byte>>> sendQueue = new();

    protected override void SendContent(Packet<ReadOnlyMemory<byte>> packet)
        => sendQueue.Post(packet);

    protected override void OnDisconnected(Exception? exception)
    {
        base.OnDisconnected(exception);

        run.Dispose();
        client.Dispose();
    }

    private async ValueTask Run(CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
        {
            return;
        }

        if (target is not null)
        {
            try
            {
                await client.ConnectAsync(target.Value.Host, target.Value.Port, Disposal);
            }
            catch (Exception exception)
            {
                if (exception is ArgumentOutOfRangeException or ArgumentNullException or SocketException)
                {
                    SetDisconnected(exception);
                    return;
                }
                else
                {
                    throw;
                }
            }
        }

        SetConnected();

        while (!client.Connected)
        {
            await Task.Delay(50, cancellation);
        }

        await Task.WhenAll(RunMonitor(cancellation), RunSend(cancellation), RunRead(cancellation));
    }

    private async Task RunMonitor(CancellationToken cancellation)
    {
        Socket socket = client.Client;
        byte[] peekBuffer = new byte[1];

        while (!cancellation.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellation);

            try
            {
                if (!socket.Connected || (socket.Poll(0, SelectMode.SelectRead) && socket.Receive(peekBuffer, SocketFlags.Peek) is 0))
                {
                    SetDisconnected(new RemoteDisconnectException());
                    return;
                }
            }
            catch (SocketException exception)
            {
                SetDisconnected(exception);
                return;
            }
        }
    }

    private async Task RunSend(CancellationToken cancellation)
    {
        NetworkStream stream = client.GetStream();

        while (!cancellation.IsCancellationRequested)
        {
            Packet<ReadOnlyMemory<byte>> packet = await sendQueue.ReceiveAsync(cancellation);
            await stream.WriteAsync(packet.Content, cancellation);
            packet.Recycle();
        }
    }

    private async Task RunRead(CancellationToken cancellation)
    {
        NetworkStream stream = client.GetStream();

        try
        {
            while (!cancellation.IsCancellationRequested)
            {
                Buffer<byte> buffer = pool.NewBuffer();
                buffer.Length = 1024;

                int read = await stream.ReadAsync(buffer.Memory, cancellation);
                if (read == 0)
                {
                    buffer.Recycle();
                    SetDisconnected(new RemoteDisconnectException());
                    return;
                }

                TriggerReceived(Packet.FromBuffer(buffer, read));
            }
        }
        catch (EndOfStreamException exception)
        {
            SetDisconnected(exception);
        }
    }

    private sealed class Info(TcpConnection connection) : IIpConnectionInfo
    {
        public bool IsOutgoing => connection.target is not null;
        public bool IsIncoming => connection.target is null;
        public string Host => connection.target?.Host ?? ((IPEndPoint)connection.client.Client.RemoteEndPoint!).Address.ToString();
        public int Port => connection.target?.Port ?? ((IPEndPoint)connection.client.Client.RemoteEndPoint!).Port;
    }
}