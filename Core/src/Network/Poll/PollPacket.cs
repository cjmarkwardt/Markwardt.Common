namespace Markwardt;

public interface IConstructable<T>
{
    static abstract T New();
}

public interface IPollPacket
{
    bool IsPoll();
}