namespace Markwardt;

public interface IMessageHoster<T>
{
    IMessageHost<T> Host();
}

public class MessageHoster<T>(Func<IMessageHost<T>> host) : IMessageHoster<T>
{
    public IMessageHost<T> Host()
        => host();
}