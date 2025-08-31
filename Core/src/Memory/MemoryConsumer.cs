namespace Markwardt;

public delegate void MemoryConsumer<T>(ReadOnlySpan<T> data);

public delegate void MemoryConsumer<T, in TArgument>(TArgument argument, ReadOnlySpan<T> data);