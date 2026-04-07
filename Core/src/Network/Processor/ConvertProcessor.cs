namespace Markwardt;

public abstract class ConvertProcessor<TSend, TReceive> : MessageProcessor<TSend, TReceive>
{
    protected sealed override void SendContent(Message message, TSend content)
    {
        TReceive converted = Convert(content);
        message.RecycleContent();
        message.Content = converted;
        TriggerSent(message);
    }

    protected sealed override void ReceiveContent(Message message, TReceive content)
    {
        TSend reverted = Revert(content);
        message.RecycleContent();
        message.Content = reverted;
        TriggerReceived(message);
    }

    protected abstract TReceive Convert(TSend content);
    protected abstract TSend Revert(TReceive content);
}