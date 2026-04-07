namespace Markwardt;

public abstract class FrontendReceiver<TMessage> : MessageReceiver<TMessage>
    where TMessage : notnull
{
    //protected override bool Filter(TMessage message, object? source)
        //=> base.Filter(message, source) && source is IFrontendWindow;
}