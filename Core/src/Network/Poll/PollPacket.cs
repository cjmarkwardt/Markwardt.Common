namespace Markwardt.Network;

public interface IConstructable<T>
{
    static abstract T New();
}

public interface IPollPacket
{
    bool IsPoll();
}