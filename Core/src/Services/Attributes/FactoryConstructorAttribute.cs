namespace Markwardt;

[AttributeUsage(AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
public class FactoryConstructorAttribute(Type result, string? constructorName = null) : Attribute
{
    public Type Result { get; } = result;
    public string? ConstructorName { get; } = constructorName;
}

public class FactoryConstructorAttribute<TResult>(string? constructorName = null) : FactoryConstructorAttribute(typeof(TResult), constructorName)
    where TResult : notnull;