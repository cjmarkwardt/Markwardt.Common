namespace Markwardt;

public class TcpBlockHoster(int port, int clampedCapacity = 1024) : INetworkHoster
{
    public INetworkListener CreateListener()
        => new Listener(port, clampedCapacity);

    private sealed class Listener(int port, int clampedCapacity) : NetworkListener
    {
        private readonly TcpListener listener = new(IPAddress.Any, port);

        public override async ValueTask Run(CancellationToken cancellation = default)
        {
            listener.Start();

            while (!cancellation.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellation);
                if (Handler is null)
                {
                    client.Dispose();
                }
                else
                {
                    Handler.OnConnected(new TcpBlockLink(client, clampedCapacity));
                }
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            listener.Dispose();
        }
    }
}