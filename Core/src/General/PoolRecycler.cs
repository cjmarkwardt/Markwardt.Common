namespace Markwardt;

public class PoolRecycler<T>(IPool<PoolRecycler<T>>? recyclerPool = null) : IRecyclable
{
    private (IPool<T> ItemPool, T Item)? target;
    
    public void Reset(IPool<T> itemPool, T item)
        => target = (itemPool, item);

    public void Recycle()
    {
        if (target is not null)
        {
            (IPool<T> itemPool, T item) = target.Value;
            target = null;
            itemPool.Recycle(item);
            recyclerPool?.Recycle(this);
        }
    }
}