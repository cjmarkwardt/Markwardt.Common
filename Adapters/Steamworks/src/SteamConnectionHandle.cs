namespace Markwardt;

internal class SteamConnectionHandle(HSteamNetConnection value) : Finalized<HSteamNetConnection>(value)
{
    private readonly nint[] readBuffer = new nint[20];

    public unsafe EResult Write(ReadOnlySpan<byte> data, int sendFlags)
    {
        fixed (byte* pointer = data)
        {
            return SteamNetworkingSockets.SendMessageToConnection(Value, (nint)pointer, (uint)data.Length, sendFlags, out _);
        }
    }

    public bool Read(Action<ReadOnlySpan<byte>> receive)
    {
        while (true)
        {
            int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(Value, readBuffer, readBuffer.Length);
            if (messageCount < 0)
            {
                return false;
            }
            else if (messageCount == 0)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < messageCount; i++)
                {
                    using SteamMessageHandle message = new(readBuffer[i]);
                    receive(message.Data);
                }
            }
        }
    }

    protected override void Release(HSteamNetConnection value)
        => SteamNetworkingSockets.CloseConnection(value, 0, string.Empty, true);
}