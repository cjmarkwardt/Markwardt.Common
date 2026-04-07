namespace Markwardt;

public interface IRecyclerPool<T>
{
    IRecyclable GetRecycler(T item);
}

public class RecyclerPool<T> : IRecyclerPool<T>
{
    public RecyclerPool(IPool<T> itemPool)
    {
        this.itemPool = itemPool;
        recyclerPool = new Pool<PoolRecycler<T>>(() => new PoolRecycler<T>(recyclerPool));
    }

    private readonly IPool<T> itemPool;
    private readonly Pool<PoolRecycler<T>> recyclerPool;

    public IRecyclable GetRecycler(T item)
    {
        PoolRecycler<T> recycler = recyclerPool.Get();
        recycler.Reset(itemPool, item);
        return recycler;
    }
}