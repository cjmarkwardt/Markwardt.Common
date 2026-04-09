namespace Markwardt;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigureServicesAttribute(Type servicePackage) : Attribute
{
    public Type ServicePackage => servicePackage;
}

[AttributeUsage(AttributeTargets.Class)]
public class ConfigureServicesAttribute<TServicePackage> : ConfigureServicesAttribute
    where TServicePackage : IServicePackage
{
    public ConfigureServicesAttribute()
        : base(typeof(TServicePackage)) { }
}