namespace Markwardt;

public abstract class ConvertProcessor<TSend, TReceive> : MessageProcessor<TSend, TReceive>
{
    protected sealed override void SendContent(Message message, TSend content)
    {
        message.SetContent(Convert(content));
        TriggerSent(message);
    }

    protected sealed override void ReceiveContent(Message message, TReceive content)
    {
        message.SetContent(Revert(content));
        TriggerReceived(message);
    }

    protected abstract TReceive Convert(TSend content);
    protected abstract TSend Revert(TReceive content);
}