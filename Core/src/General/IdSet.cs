namespace Markwardt;

public class IdSet(int start, TimeSpan reuseDelay)
{
    public IdSet()
        : this(1, TimeSpan.FromMinutes(1)) { }

    private readonly HashSet<int> activeIds = [];
    private readonly Queue<ReleasedId> releasedIds = [];

    private int edge = start;

    public int Next()
    {
        int id;
        if (releasedIds.TryPeek(out ReleasedId releasedId) && releasedId.ReuseTime <= DateTime.UtcNow)
        {
            releasedIds.Dequeue();
            id = releasedId.Id;
        }
        else
        {
            id = edge++;
        }

        activeIds.Add(id);
        return id;
    }

    public void Release(int id)
    {
        activeIds.Remove(id);
        releasedIds.Enqueue(new ReleasedId(id, DateTime.UtcNow + reuseDelay));
    }

    private record struct ReleasedId(int Id, DateTime ReuseTime);
}