namespace Markwardt;

public class SteamConnectionHandle(HSteamNetConnection value) : Finalized<HSteamNetConnection>(value)
{
    public unsafe EResult Write(ReadOnlySpan<byte> data, int sendFlags)
    {
        fixed (byte* pointer = data)
        {
            return SteamNetworkingSockets.SendMessageToConnection(Value, (nint)pointer, (uint)data.Length, sendFlags, out _);
        }
    }

    public bool Read(nint[] buffer, MemoryConsumer<byte> receive)
    {
        int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(Value, buffer, buffer.Length);
        if (messageCount < 0)
        {
            return false;
        }
        else if (messageCount > 0)
        {
            for (int i = 0; i < messageCount; i++)
            {
                using SteamMessage message = new(buffer[i]);
                message.Read(receive);
            }
        }

        return true;
    }

    protected override void Release(HSteamNetConnection value)
        => SteamNetworkingSockets.CloseConnection(value, 0, string.Empty, true);
}