namespace Markwardt.Network;

public interface IHoster<T>
{
    IHost<T> Host();
}

public class Hoster<T>(Func<IHost<T>> host) : IHoster<T>
{
    public IHost<T> Host()
        => host();
}