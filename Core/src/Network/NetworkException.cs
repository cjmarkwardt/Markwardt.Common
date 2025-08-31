namespace Markwardt;

public class NetworkException(string? message = null, Exception? innerException = null) : Exception(message, innerException);