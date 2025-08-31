namespace Markwardt;

public interface IMemorySink<T>
{
    IDisposable Subscribe(MemoryConsumer<T> action);
}

public interface IMemorySink<T, TArgument>
{
    IDisposable Subscribe(MemoryConsumer<T, TArgument> action);
}

public class MemorySink<T> : IMemorySink<T>
{
    private readonly List<MemoryConsumer<T>> actions = [];

    public void Invoke(ReadOnlySpan<T> data)
    {
        foreach (MemoryConsumer<T> action in actions)
        {
            action(data);
        }
    }

    public IDisposable Subscribe(MemoryConsumer<T> action)
    {
        actions.Add(action);
        return Disposable.Create(() => actions.Remove(action));
    }
}

public class MemorySink<T, TArgument> : IMemorySink<T, TArgument>
{
    private readonly List<MemoryConsumer<T, TArgument>> actions = [];

    public void Invoke(ReadOnlySpan<T> data, TArgument argument)
    {
        foreach (MemoryConsumer<T, TArgument> action in actions)
        {
            action(argument, data);
        }
    }

    public IDisposable Subscribe(MemoryConsumer<T, TArgument> action)
    {
        actions.Add(action);
        return Disposable.Create(() => actions.Remove(action));
    }
}