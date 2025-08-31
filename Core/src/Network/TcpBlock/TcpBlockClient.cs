namespace Markwardt;

public class TcpBlockClient : NetworkClient
{
    public TcpBlockClient(string host, int port, int clampedCapacity = 2048)
    {
        this.host = host;
        this.port = port;
        connection = new(this, new(), clampedCapacity);
    }

    private readonly string host;
    private readonly int port;
    private readonly TcpBlockConnection connection;

    protected override async ValueTask ExecuteSend(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
        => await connection.Send(data, cancellation);

    protected override async ValueTask Run(CancellationToken cancellation)
        => await connection.Run(cancellation);

    protected override async ValueTask Link(CancellationToken cancellation)
        => await connection.Connect(host, port, cancellation);

    protected override void Release()
        => connection.Dispose();
}