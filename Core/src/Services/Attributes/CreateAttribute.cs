namespace Markwardt;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute(Type implementation) : Attribute
{
    public Type Implementation => implementation;
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class CreateAttribute<TImplementation>() : CreateAttribute(typeof(TImplementation));