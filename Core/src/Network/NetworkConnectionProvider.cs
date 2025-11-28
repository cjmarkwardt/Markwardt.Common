namespace Markwardt;

public interface INetworkConnectionProvider : INetworkChannelProvider
{
    INetworkAuthenticator Authenticator { get; }
    INetworkFormatReceiver Receiver { get; }
    INetworkTracker<INetworkConnection> ConnectionTracker { get; }
}