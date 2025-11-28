namespace Markwardt;

public class IdRange(int start = 0)
{
    private readonly Queue<ReleasedId> releasedIds = [];

    private int edge = start;

    public TimeSpan ReuseDelay { get; set; }

    public int Next()
    {
        if (releasedIds.TryPeek(out ReleasedId? releasedId) && releasedId.IsAvailable(ReuseDelay))
        {
            return releasedIds.Dequeue().Id;
        }
        else
        {
            return edge++;
        }
    }

    public void Release(int id)
        => releasedIds.Enqueue(new(id, DateTime.Now));

    private sealed record ReleasedId(int Id, DateTime Timestamp)
    {
        public bool IsAvailable(TimeSpan? delay)
            => delay is null || (DateTime.Now - Timestamp) >= delay;
    }
}