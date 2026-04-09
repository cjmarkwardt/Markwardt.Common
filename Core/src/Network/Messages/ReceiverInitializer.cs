namespace Markwardt.Network;

public class ReceiverInitializerTag : ConstructorTag<ReceiverInitializer>;

public class ReceiverInitializer : Initializer
{
    [Inject<ReceivedPacketsTag>]
    public required IObservable<Packet> Messages { get; init; }

    [Inject<ReceiverSourceTag>]
    public required IItemSource<IReceiver> Receivers { get; init; }

    protected override void OnInitialize()
        => Messages.Subscribe(packet => Receivers.Items.ForEach(handler => handler.Receive(packet)));
}