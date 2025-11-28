namespace Markwardt;

public interface INetworkHandler
{
    /// <summary> Handles a request to register a new user, handed off to OnConnection when successful. </summary>
    /// <param name="host">Network host that the request is coming from.</param>
    /// <param name="identifier">Identifier for the new user.</param>
    /// <param name="verifier">Verifier that will be used to authenticate the new user.</param>
    /// <param name="details">Details of the registration that are publicly visible and not secure.</param>
    /// <param name="isLocal">True if the request is coming from the local machine, otherwise false.</param>
    /// <returns>Profile of the new user that will be stored with the new connection.</returns>
    /// <exception cref="NetworkException">Thrown if the registration fails.</exception>
    ValueTask<object?> OnRegistration(INetworkHost? host, string identifier, ReadOnlyMemory<byte> verifier, object? details, bool isLocal, CancellationToken cancellation);

    /// <summary> Handles a request to authenticate an existing user, handed off to OnConnection when successful. </summary>
    /// <param name="host">Network host that the request is coming from.</param>
    /// <param name="identifier">Identifier for the user.</param>
    /// <param name="isLocal">True if the request is coming from the local machine, otherwise false.</param>
    /// <returns>Profile of the existing user that will be stored with the new connection, and the verifier that will be used for authentication.</returns>
    /// <exception cref="NetworkException">Thrown if the authentication fails.</exception>
    ValueTask<(object? UserProfile, ReadOnlyMemory<byte> Verifier)> OnAuthentication(INetworkHost? host, string identifier, bool isLocal, CancellationToken cancellation);

    /// <summary> Handles an incoming connection. </summary>
    /// </summary>
    /// <param name="host">Network host that the connection is coming from.</param>
    /// <param name="user">User that is connecting.</param>
    /// <param name="request">Message that was sent with the connection.</param>
    /// <param name="isLocal">True if the connection is coming from the local machine, otherwise false.</param>
    /// <param name="isSecure">True if the connection is secure and the request/response are encrypted, otherwise false.</param>
    /// <exception cref="NetworkException">Thrown if the connection is rejected.</exception>
    ValueTask<(object? Response, object? ConnectionProfile)> OnConnection(INetworkHost? host, NetworkUser? user, object? request, bool isLocal, bool isSecure, CancellationToken cancellation);
    
    void OnConnected(INetworkConnection connection, object? message);
    void OnReceived(INetworkConnection connection, object? channel, object message);
    ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> OnRequested(INetworkConnection connection, object message, CancellationToken cancellation);
    (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) OnChannelOpened(INetworkConnection connection, object message);
    void OnChannelClosed(INetworkConnection connection, object? profile);
    void OnDisconnected(INetworkConnection connection, Exception? exception);
    void OnRecycled(object message);
}