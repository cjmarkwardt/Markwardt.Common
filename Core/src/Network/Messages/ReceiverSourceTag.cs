namespace Markwardt.Network;

[ServiceType<IItemSource<IReceiver>>]
public class ReceiverSourceTag : ConstructorTag<MassInstantiatorSource<IReceiver>>;