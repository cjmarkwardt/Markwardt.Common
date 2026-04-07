namespace Markwardt;

public class MassInstantiatorSource<T> : IItemSource<T>
{
    public MassInstantiatorSource()
        => items = new(GetItems);

    public required IMassInstantiator Instantiator { get; init; }

    private readonly Lazy<List<T>> items;
    public IReadOnlyList<T> Items => items.Value;

    protected virtual (bool IsMatch, object? FilterData) Filter(Type type)
        => (type.IsAssignableTo(typeof(T)) && !type.IsAbstract, null);

    protected virtual T CreateItem(object instance, object? filterData)
        => (T)instance;

    private List<T> GetItems()
        => Instantiator.CreateInstances(Filter).Select(x => CreateItem(x.Instance, x.FilterData)).ToList();
}