namespace Markwardt;

[DataContract]
public record StandardPacket<T> : IPollPacket, IRequestPacket, IChannelPacket
{
    [DataMember(Order = 1)]
    public int Request { get; set; }

    [DataMember(Order = 2)]
    public int Channel { get; set; }

    [DataMember(Order = 3)]
    public int Sequence { get; set; }

    [DataMember(Order = 4)]
    public T? Content { get; set; }

    public void SetRequest(int request)
    {
        Request = request;
        Channel = default;
        Sequence = default;
    }

    public int GetRequest()
        => Request;

    public void SetChannel(MessageChannelHeader header)
    {
        Request = default;
        Channel = header.Channel;
        Sequence = header.Sequence;
    }

    public Maybe<MessageChannelHeader> GetChannel()
        => Channel > 0 ? new MessageChannelHeader(Channel, Sequence).Maybe() : default;

    public void SetContent(T content)
        => Content = content;

    public T GetContent()
        => Content ?? throw new InvalidOperationException();

    public bool IsPoll()
        => Content is null;
}