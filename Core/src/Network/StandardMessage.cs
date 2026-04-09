namespace Markwardt.Network;

[DataContract]
public record StandardMessage<T> : IPollPacket, IHeaderPacket<RequestHeader>, IHeaderPacket<ChannelHeader>, IConstructable<StandardMessage<T>>, IRecyclable
{
    private readonly static Pool<StandardMessage<T>> pool = new(() => new());

    public static StandardMessage<T> New()
        => pool.Get();

    public static StandardMessage<T> New(T content)
    {
        StandardMessage<T> packet = New();
        packet.Content = content;
        return packet;
    }

    [DataMember(Order = 1)]
    public int Request { get; set; }

    [DataMember(Order = 2)]
    public int Response { get; set; }

    [DataMember(Order = 3)]
    public int Channel { get; set; }

    [DataMember(Order = 4)]
    public ChannelPart ChannelPart { get; set; }

    [DataMember(Order = 5)]
    public int Sequence { get; set; }

    [DataMember(Order = 6)]
    public T? Content { get; set; }

    public void Recycle()
    {
        Request = default;
        Response = default;
        Channel = default;
        ChannelPart = default;
        Sequence = default;
        Content = default;

        pool.Recycle(this);
    }

    bool IPollPacket.IsPoll()
        => Content is null && ChannelPart is not ChannelPart.Close;

    Maybe<RequestHeader> IHeaderPacket<RequestHeader>.GetHeader()
        => Request == 0 && Response == 0 ? default : new RequestHeader(Request != 0 ? RequestFlow.Request : RequestFlow.Response, Request != 0 ? Request : Response).Maybe();

    void IHeaderPacket<RequestHeader>.SetHeader(RequestHeader header)
    {
        Request = header.Flow is RequestFlow.Request ? header.Id : 0;
        Response = header.Flow is RequestFlow.Response ? header.Id : 0;
    }

    Maybe<ChannelHeader> IHeaderPacket<ChannelHeader>.GetHeader()
        => Channel == 0 ? default : new ChannelHeader(Channel, ChannelPart, Sequence).Maybe();

    void IHeaderPacket<ChannelHeader>.SetHeader(ChannelHeader header)
    {
        Channel = header.Channel;
        ChannelPart = header.Part;
        Sequence = header.Sequence;
    }
}