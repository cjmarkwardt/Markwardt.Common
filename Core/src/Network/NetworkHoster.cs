namespace Markwardt;

public interface INetworkHoster
{
    INetworkListener CreateListener();
}