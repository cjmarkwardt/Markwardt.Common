namespace Markwardt;

public interface IPool
{
    object Rent(Type type);

    void Return(object value);
    
    void Clear(Type type);

    void Clear();
}

public interface IPool<T>
{
    T Rent();
    void Return(T value);
    void Clear();
}

public static class PoolExtensions
{
    public static T Rent<T>(this IPool pool)
        where T : notnull
        => (T)pool.Rent(typeof(T));

    public static void Clear<T>(this IPool pool)
        where T : notnull
        => pool.Clear(typeof(T));
}

public class Pool(Func<Type, object> create, Action<object>? reset = null) : IPool
{
    public static Pool Shared { get; } = new(x => Activator.CreateInstance(x) ?? throw new InvalidOperationException(), x => (x as IResettable)?.Reset());

    private readonly Dictionary<Type, Pool<object>> pools = [];

    public object Rent(Type type)
    {
        if (!pools.TryGetValue(type, out Pool<object>? pool))
        {
            pool = new(() => create(type), reset);
            pools.Add(type, pool);
        }

        return pool.Rent();
    }

    public void Return(object value)
    {
        if (pools.TryGetValue(value.GetType(), out Pool<object>? pool))
        {
            pool.Return(value);
        }
    }

    public void Clear(Type type)
        => pools.Remove(type);

    public void Clear()
        => pools.Clear();
}

public class Pool<T>(Func<T> create, Action<T>? reset = null) : IPool<T>
    where T : notnull
{
    private readonly Queue<T> queue = [];

    public T Rent()
    {
        if (queue.TryDequeue(out T? value))
        {
            return value;
        }
        else
        {
            return create();
        }
    }

    public void Return(T value)
    {
        reset?.Invoke(value);
        queue.Enqueue(value);
    }

    public void Clear()
        => queue.Clear();
}