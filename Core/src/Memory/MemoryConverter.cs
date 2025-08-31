namespace Markwardt;

public delegate TResult MemoryConverter<T, TResult>(ReadOnlySpan<T> data);