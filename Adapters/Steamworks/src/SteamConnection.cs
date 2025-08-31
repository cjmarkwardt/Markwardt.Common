namespace Markwardt;

public class SteamConnection(INetworkConnectionController controller) : BaseDisposable
{
    public SteamConnection(INetworkConnectionController controller, SteamConnectionHandle handle)
        : this(controller)
        => this.handle = handle;

    private SteamConnectionHandle? handle;
    
    public HSteamNetConnection Handle => handle.NotNull().Value;

    public void Initialize(SteamConnectionHandle handle)
    {
        if (this.handle is not null)
        {
            throw new InvalidOperationException("Already initialized");
        }

        this.handle = handle;
    }

    public async ValueTask Run(CancellationToken cancellation)
    {
        nint[] readBuffer = new nint[100];

        while (!cancellation.IsCancellationRequested)
        {
            if (handle is not null && !handle.Read(readBuffer, controller.Receive))
            {
                throw new InvalidOperationException("Failed to receive messages");
            }

            await Task.Delay(25, cancellation);
        }
    }

    public void Send(ReadOnlySpan<byte> data, NetworkConstraints constraints)
    {
        EResult result = handle!.Write(data, constraints == NetworkConstraints.None ? 0 : 8);
        if (result is not EResult.k_EResultOK)
        {
            controller.Drop(new InvalidOperationException($"Failed to send ({result})"));
        }
    }

    protected override void OnDispose()
        => handle?.Dispose();
}