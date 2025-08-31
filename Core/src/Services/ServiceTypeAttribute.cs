namespace Markwardt;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceTypeAttribute(Type type) : Attribute
{
    public Type Type => type;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceTypeAttribute<T>() : ServiceTypeAttribute(typeof(T));