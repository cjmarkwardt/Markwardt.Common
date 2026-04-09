namespace Markwardt;

public interface IMessageAsserter : IMessageSender
{
    void Send();
}

public interface IMessageAsserter<T> : IMessageAsserter, IMessageSender<T>;

public class MessageAsserter<T>(IMessageAsserter asserter) : IMessageAsserter<T>
{
    public void Send()
        => asserter.Send();

    public void Send(Message message)
        => asserter.Send(message);
}