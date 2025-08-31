namespace Markwardt;

public class IdRange(int start = 0)
{
    private readonly HashSet<int> activeIds = [];
    private readonly Queue<int> releasedIds = [];

    private int edge = start;

    public int Next()
    {
        if (!releasedIds.TryDequeue(out int id))
        {
            id = edge;
            edge++;
        }

        activeIds.Add(id);
        return id;
    }

    public void Release(int id)
    {
        activeIds.Remove(id);
        releasedIds.Enqueue(id);
    }
}