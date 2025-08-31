namespace Markwardt;

public class SteamMessage(nint value) : Finalized<nint>(value)
{
    private readonly SteamNetworkingMessage_t message = SteamNetworkingMessage_t.FromIntPtr(value);

    public unsafe void Read(MemoryConsumer<byte> consumer)
        => consumer(new ReadOnlySpan<byte>((void*)message.m_pData, message.m_cbSize));

    protected override void Release(nint value)
        => SteamNetworkingMessage_t.Release(value);
}