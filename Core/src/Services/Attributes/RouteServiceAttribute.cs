namespace Markwardt;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class RouteServiceAttribute(Type tag) : ServiceResolutionAttribute
{
    public override IServiceSource GetSource(Type type)
        => ServiceSource.FromRoute(tag);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class RouteServiceAttribute<TService>() : RouteServiceAttribute(typeof(TService))
    where TService : notnull;