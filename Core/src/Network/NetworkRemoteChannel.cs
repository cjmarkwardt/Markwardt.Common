namespace Markwardt;

public interface INetworkRemoteChannel : INetworkGroupChannel
{
    void RemoteSync(INetworkRemoteConnection connection, byte sequence);
    void TryResend();
}