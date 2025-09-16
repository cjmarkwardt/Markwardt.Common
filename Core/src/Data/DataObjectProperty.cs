namespace Markwardt;

public record DataObjectProperty(int Index, string Name, Type Type)
{
    private readonly Func<object> creator = ObservableTarget.GetCreator(Type);

    public bool IsValue { get; } = ObservableTarget.IsValue(Type);
    public bool IsCollection { get; } = ObservableTarget.IsCollection(Type);
    public bool IsDictionary { get; } = ObservableTarget.IsDictionary(Type);

    public object Create()
        => creator();
}