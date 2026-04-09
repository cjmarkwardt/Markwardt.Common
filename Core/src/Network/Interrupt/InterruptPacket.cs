namespace Markwardt.Network;

[DataContract]
public record InterruptPacket
{
    public static InterruptPacket FromData(bool isStart, bool isEnd, int priority, ReadOnlyMemory<byte> data)
    {
        InterruptHeader type;
        if (isStart && isEnd)
        {
            type = InterruptHeader.Unit;
        }
        else if (isStart)
        {
            type = InterruptHeader.Start;
        }
        else if (isEnd)
        {
            type = InterruptHeader.End;
        }
        else
        {
            type = InterruptHeader.Continue;
        }

        return new InterruptPacket() { Type = type, Priority = priority, Data = data };
    }

    [property: DataMember(Order = 1)]
    public InterruptHeader Type { get; init; }

    [property: DataMember(Order = 2)]
    public int Priority { get; init; }

    [property: DataMember(Order = 3)]
    public ReadOnlyMemory<byte> Data { get; init; }
}