namespace Markwardt;

public record DataObjectProperty(int Index, string Name, Type Type)
{
    private readonly Func<IAccessible> creator = ObservableTarget.GetCreator(Type);

    public IAccessible Create()
        => creator();
}