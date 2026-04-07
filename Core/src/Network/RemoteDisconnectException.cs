namespace Markwardt;

public class RemoteDisconnectException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public RemoteDisconnectException()
        : this("Lost remote connection") { }
}