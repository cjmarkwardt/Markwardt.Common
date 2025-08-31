namespace Markwardt;

public abstract class EventTag : ImplementationTag<Event>;

public abstract class EventTag<T> : ImplementationTag<Event<T>>;