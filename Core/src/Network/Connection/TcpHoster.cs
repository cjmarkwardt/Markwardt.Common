namespace Markwardt;

public class IpPortKey : InspectValueKey<int>
{
    public static IpPortKey Instance { get; } = new();
    
    private IpPortKey()
        : base(nameof(IpPortKey)) { }
}

public class TcpHoster(int? port = null, IPAddress? address = null, MemoryPool<byte>? pool = null) : IMessageHoster<ReadOnlyMemory<byte>>
{
    public IMessageHost<ReadOnlyMemory<byte>> Host()
        => new Server(port, address, pool);

    private sealed class Server : BaseMessageHost<ReadOnlyMemory<byte>>
    {
        public Server(int? port, IPAddress? address, MemoryPool<byte>? pool)
        {
            this.pool = pool;
            listener = new(address ?? IPAddress.Any, port ?? 0);

            listener.Start();
            Inspectable.SetInspect(IpPortKey.Instance, ((IPEndPoint)listener.LocalEndpoint).Port);

            this.RunInBackground(Run);
        }

        private readonly TcpListener listener;
        private readonly MemoryPool<byte>? pool;

        private async ValueTask Run(CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    Enqueue(new TcpConnection(await listener.AcceptTcpClientAsync(cancellation), null, pool));
                }
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}