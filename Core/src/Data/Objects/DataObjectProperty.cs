namespace Markwardt;

public record DataObjectProperty(int Index, string Name, Type Type)
{
    public DataCollectionType? CollectionType { get; } = DataCollectionType.Scan(Type);
}