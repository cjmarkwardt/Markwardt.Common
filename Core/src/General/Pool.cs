namespace Markwardt;

public interface IPool<T>
{
    T Get();
    void Recycle(T item);
}

public static class PoolExtensions
{
    public static IRecyclerPool<T> CreateRecyclerPool<T>(this IPool<T> pool)
        => new RecyclerPool<T>(pool);
}

public class Pool<T>(Func<T> factory, Action<T>? reset = null, int? retention = null) : IPool<T>
{
    private readonly Queue<T> items = [];

    public T Get()
    {
        if (!items.TryDequeue(out T? item))
        {
            item = factory();
        }

        return item;
    }

    public void Recycle(T item)
    {
        if (items.Count < retention)
        {
            reset?.Invoke(item);
            items.Enqueue(item);
        }
    }
}