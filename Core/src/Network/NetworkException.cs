namespace Markwardt;

public class NetworkException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public static NetworkException Unhandled => new("Unhandled network operation");
}