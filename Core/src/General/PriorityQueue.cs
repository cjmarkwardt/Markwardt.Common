namespace Markwardt;

public interface IPriorityQueue<T> : ICollection<T>
{
    int HighestPriority { get; }

    void AddWithPriority(T item, int priority);
    Maybe<T> Peek(int? priority = null);
    Maybe<T> Dequeue(int? priority = null);
}

public class PriorityQueue<T> : IPriorityQueue<T>
{
    private readonly Dictionary<int, LinkedList<T>> queues = [];

    public int Count => queues.Values.Sum(x => x.Count);

    bool ICollection<T>.IsReadOnly => false;

    public int HighestPriority => queues.Where(x => x.Value.Count > 0).Select(x => x.Key).DefaultIfEmpty(0).Max();

    public void Add(T item)
        => AddWithPriority(item, (item as IPrioritizable)?.Priority ?? 0);

    public void AddWithPriority(T item, int priority)
    {
        if (!queues.TryGetValue(priority, out LinkedList<T>? queue))
        {
            queue = [];
            queues[priority] = queue;
        }

        queue.AddLast(item);
    }

    public void Clear()
        => queues.Clear();

    public bool Contains(T item)
        => queues.Values.Any(x => x.Contains(item));

    public void CopyTo(T[] array, int arrayIndex)
        => this.ForEach((x, index) => array[arrayIndex + index] = x);

    public IEnumerator<T> GetEnumerator()
        => queues.OrderByDescending(x => x.Key).SelectMany(x => x.Value).GetEnumerator();

    public Maybe<T> Peek(int? priority = null)
    {
        if (queues.TryGetValue(priority ?? HighestPriority, out LinkedList<T>? queue) && queue.First is not null)
        {
            return queue.First.Value.Maybe();
        }

        return default;
    }

    public Maybe<T> Dequeue(int? priority = null)
    {
        if (queues.TryGetValue(priority ?? HighestPriority, out LinkedList<T>? queue) && queue.First is not null)
        {
            T value = queue.First.Value;
            queue.RemoveFirst();
            return value.Maybe();
        }

        return default;
    }

    public bool Remove(T item)
    {
        bool removed = false;
        foreach (LinkedList<T> queue in queues.Values)
        {
            if (queue.Remove(item))
            {
                removed = true;
            }
        }

        return removed;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}