namespace Markwardt;

public interface IInspectKey
{
    string Name { get; }
    Type Type { get; }
}

public class InspectKey<T>(string name) : IInspectKey
    where T : class
{
    public string Name => name;
    public Type Type => typeof(T);
}