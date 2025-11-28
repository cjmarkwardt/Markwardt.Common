namespace Markwardt;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute(Type? tag = null) : ServiceAttribute
{
    public override IService GetService(Type type)
        => Service.Route(tag ?? type.GetDefaultImplementation());
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute<TService>() : InjectAttribute(typeof(TService))
    where TService : notnull;