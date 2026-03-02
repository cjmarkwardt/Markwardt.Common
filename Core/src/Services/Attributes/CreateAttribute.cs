namespace Markwardt;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute(Type? implementation = null, string? constructorName = null) : ServiceAttribute
{
    public override IService GetService(Type type)
        => new ConstructorService(implementation ?? type.GetDefaultImplementation(), constructorName);

    public override string ToString()
    {
        StringBuilder builder = new(base.ToString());

        if (implementation is not null)
        {
            builder.Append($" (Implementation: {implementation})");
        }

        if (constructorName is not null)
        {
            builder.Append($" (Constructor: {constructorName})");
        }

        return builder.ToString();
    }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute<TImplementation>(string? constructorName = null) : CreateAttribute(typeof(TImplementation), constructorName);