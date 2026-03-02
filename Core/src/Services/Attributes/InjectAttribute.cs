namespace Markwardt;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute(Type? tag = null) : ServiceAttribute
{
    public override IService GetService(Type type)
        => new RouteService(tag ?? type.GetDefaultImplementation());

    public override string ToString()
    {
        StringBuilder builder = new(base.ToString());

        if (tag is not null)
        {
            builder.Append($" (Tag: {tag})");
        }

        return builder.ToString();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Parameter)]
public class InjectAttribute<TService>() : InjectAttribute(typeof(TService))
    where TService : notnull;