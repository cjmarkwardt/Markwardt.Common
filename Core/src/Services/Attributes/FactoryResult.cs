namespace Markwardt;

[AttributeUsage(AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
public class FactoryResultAttribute(Type result) : Attribute
{
    public Type Result { get; } = result;
}

public class FactoryResultAttribute<TResult>() : FactoryResultAttribute(typeof(TResult))
    where TResult : notnull;