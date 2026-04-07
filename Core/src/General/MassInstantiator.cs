namespace Markwardt;

public interface IMassInstantiator
{
    IEnumerable<(object Instance, object? FilterData)> CreateInstances(Func<Type, (bool IsMatch, object? FilterData)> filter);
}

public class MassInstantiator : IMassInstantiator
{
    public required IServiceProvider Services { get; init; }

    public IEnumerable<(object Instance, object? FilterData)> CreateInstances(Func<Type, (bool IsMatch, object? FilterData)> filter)
    {
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
        {
            bool isMatch = true;
            object? filterData = null;

            if (filter is not null)
            {
                (isMatch, filterData) = filter(type);
            }

            if (isMatch)
            {
                yield return (Services.Create(type), filterData);
            }
        }
    }
}