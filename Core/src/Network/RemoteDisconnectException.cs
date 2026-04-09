namespace Markwardt.Network;

public class RemoteDisconnectException(string packet, Exception? innerException = null) : Exception(packet, innerException)
{
    public RemoteDisconnectException()
        : this("Lost remote connection") { }
}