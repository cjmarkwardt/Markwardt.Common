namespace Markwardt;

public abstract class SignalTag : ConstructorTag<Signal>;

public abstract class SignalTag<T> : ConstructorTag<Signal<T>>;