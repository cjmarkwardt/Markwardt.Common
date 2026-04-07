namespace Markwardt;

[ServiceType<IItemSource<IMessageReceiver>>]
public class MessageReceiverSourceTag : ConstructorTag<MassInstantiatorSource<IMessageReceiver>>;