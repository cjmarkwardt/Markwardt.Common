namespace Markwardt;

public interface IDataReferenceCollector
{
    bool Collect(object value);
}

public class DataReferenceCollector : IDataReferenceCollector, IDataReferenceSource
{
    private readonly HashSet<object> pending = [];
    private readonly Dictionary<object, int> references = [];

    private int nextReference = 0;

    public bool Collect(object value)
    {
        if (pending.Remove(value))
        {
            references[value] = nextReference++;
        }
        else if (!references.ContainsKey(value))
        {
            pending.Add(value);
            return true;
        }

        return false;
    }

    public int? Get(object value)
        => references.TryGetValue(value, out int reference) ? reference : null;
}