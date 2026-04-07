namespace Markwardt;

public class MessagesInitializerTag : ConstructorTag<MessagesInitializer>;

public class MessagesInitializer : Initializer
{
    [Inject<MessagesTag>]
    public required IObservable<Message> Messages { get; init; }

    [Inject<MessageReceiverSourceTag>]
    public required IItemSource<IMessageReceiver> Receivers { get; init; }

    protected override void OnInitialize()
        => Messages.Subscribe(message => Receivers.Items.ForEach(handler => handler.Receive(message)));
}