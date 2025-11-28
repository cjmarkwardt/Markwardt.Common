namespace Markwardt;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute(Type? implementation = null, string? constructorName = null) : ServiceAttribute
{
    public override IService GetService(Type type)
        => Service.Constructor(implementation ?? type.GetDefaultImplementation(), constructorName);
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute<TImplementation>(string? constructorName = null) : CreateAttribute(typeof(TImplementation), constructorName);