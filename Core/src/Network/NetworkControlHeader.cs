namespace Markwardt;

public enum NetworkControlHeader
{
    Connect,
    CompleteConnect,
    Secure,
    CreateSession,
    StartSession,
    Register,
    Authenticate,
    Disconnect,
    RejectRequest,
    OpenChannel,
    CloseChannel
}