namespace Markwardt;

public class TcpBlockServer(int port, int clampedCapacity = 2048) : NetworkServer<TcpBlockConnection>
{
    private readonly TcpListener listener = new(IPAddress.Any, port);

    protected override async ValueTask Run(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            if (IsOpen)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellation);
                Connect(x => new TcpBlockConnection(x, client, clampedCapacity));
            }
            else
            {
                await Task.Delay(25, cancellation);
            }
        }
    }

    protected override ValueTask Link(CancellationToken cancellation)
    {
        listener.Start();
        return ValueTask.CompletedTask;
    }

    protected override async ValueTask RunConnection(TcpBlockConnection connection, CancellationToken cancellation)
        => await connection.Run(cancellation);

    protected override async ValueTask ExecuteSend(TcpBlockConnection connection, ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
        => await connection.Send(data, cancellation);

    protected override void ReleaseConnection(TcpBlockConnection connection)
        => connection.Dispose();

    protected override void Release()
    {
        base.Release();
        listener.Dispose();
    }
}