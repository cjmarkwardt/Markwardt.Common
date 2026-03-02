namespace Markwardt;

public record struct ServiceParameter(ServiceParameterType ParameterType, Type Type, string Name, ICustomAttributeProvider Attributes)
{
    public override string ToString()
        => $"{Name} ({ParameterType})";
}

public delegate IService? ServiceOverride(ServiceParameter parameter);