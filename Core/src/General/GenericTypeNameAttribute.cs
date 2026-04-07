namespace Markwardt;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true)]
public class GenericTypeNameAttribute(Type target, string name) : Attribute
{
    public static string GetName(Type type)
        => type.GetCustomAttributes<GenericTypeNameAttribute>().FirstOrDefault(x => x.Target == type)?.Name ?? throw new InvalidOperationException($"Type {type} does not have a generic type name");

    public Type Target => target;
    public string Name => name;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = true)]
public class GenericTypeNameAttribute<T>(string name) : GenericTypeNameAttribute(typeof(T), name);