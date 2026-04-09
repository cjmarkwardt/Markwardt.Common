namespace Markwardt.Network;

public class ConnectedSignal
{
    public static ConnectedSignal Instance { get; } = new();

    private ConnectedSignal() { }
}