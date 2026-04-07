namespace Markwardt;

internal class SteamMessageHandle(nint value) : Finalized<nint>(value)
{
    private readonly SteamNetworkingMessage_t message = SteamNetworkingMessage_t.FromIntPtr(value);

    public unsafe ReadOnlySpan<byte> Data => new((void*)message.m_pData, message.m_cbSize);

    protected override void Release(nint value)
        => SteamNetworkingMessage_t.Release(value);
}