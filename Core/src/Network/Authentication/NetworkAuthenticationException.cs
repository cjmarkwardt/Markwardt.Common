namespace Markwardt;

public class NetworkAuthenticationException(string? message = null, Exception? innerException = null) : NetworkException(message ?? "Authentication failed", innerException);