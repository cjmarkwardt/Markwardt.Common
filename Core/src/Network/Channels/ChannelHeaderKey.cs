namespace Markwardt;

public class ChannelHeaderKey : InspectValueKey<MessageChannelHeader>
{
    public static ChannelHeaderKey Instance { get; } = new();
    
    private ChannelHeaderKey()
        : base(nameof(ChannelHeaderKey)) { }
}