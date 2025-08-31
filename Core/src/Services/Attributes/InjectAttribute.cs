namespace Markwardt;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute(Type? service = null) : Attribute
{
    public Type? Service { get; } = service;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute<TService>() : InjectAttribute(typeof(TService))
    where TService : notnull;