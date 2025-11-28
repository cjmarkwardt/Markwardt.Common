namespace Markwardt;

public abstract class EventTag : ConstructorTag<Event>;

public abstract class EventTag<T> : ConstructorTag<Event<T>>;