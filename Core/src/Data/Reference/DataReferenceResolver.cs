namespace Markwardt;

public interface IDataReferenceResolver
{
    void Set(int reference, object value);
    object Resolve(int reference);
}

public class DataReferenceResolver : IDataReferenceResolver
{
    private readonly Dictionary<int, object> references = [];

    public void Set(int reference, object value)
    {
        if (references.ContainsKey(reference))
        {
            throw new InvalidOperationException($"Reference {reference} already set.");
        }

        references[reference] = value;
    }

    public object Resolve(int reference)
        => references[reference];
}