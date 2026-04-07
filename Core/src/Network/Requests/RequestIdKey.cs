namespace Markwardt;

public class RequestIdKey : InspectValueKey<int>
{
    public static RequestIdKey Instance { get; } = new();
    
    private RequestIdKey()
        : base(nameof(RequestIdKey)) { }
}