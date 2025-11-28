namespace Markwardt;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Delegate)]
public class DefaultImplementationAttribute(Type implementation) : Attribute
{
    public Type Implementation => implementation;
}

public class DefaultImplementationAttribute<TImplementation>() : DefaultImplementationAttribute(typeof(TImplementation));