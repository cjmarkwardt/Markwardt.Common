namespace Markwardt;

public static class ConstructorExtensions
{
    public static string? GetName(this ConstructorInfo constructor)
        => constructor.GetCustomAttribute<NamedConstructorAttribute>()?.Name;

    public static Type GetDefaultImplementation(this Type type)
    {
        if (type.GetCustomAttribute<DefaultImplementationAttribute>().TryNotNull(out DefaultImplementationAttribute attribute))
        {
            return attribute.Implementation;
        }
        else if (type.IsInterface && type.Name.StartsWith('I'))
        {
            string name = type.Name[1..];
            if (type.Namespace is not null)
            {
                name = $"{type.Namespace}.{name}";
            }

            if (type.Assembly.GetType(name) is Type implementation)
            {
                return implementation;
            }
        }

        return type;
    }

    public static MethodBase? GetDefaultConstructor(this Type type)
    {
        type = type.GetDefaultImplementation();

        bool IsCopyConstructor(ConstructorInfo constructor)
            => constructor.GetParameters().Length == 1 && constructor.GetParameters()[0].ParameterType == type;

        IEnumerable<MethodBase> methods = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Where(c => !IsCopyConstructor(c)).Cast<MethodBase>().Concat(type.GetMethods(BindingFlags.Static | BindingFlags.Public));
        return methods.FirstOrDefault(x => Attribute.IsDefined(x, typeof(DefaultConstructorAttribute))) ?? methods.OfType<ConstructorInfo>().OrderByDescending(m => m.GetParameters().Length).FirstOrDefault();
    }

    public static MethodBase? FindConstructor(this Type type, string? name)
        => name is null ? type.GetDefaultConstructor() : type.GetDefaultImplementation().GetConstructors().FirstOrDefault(x => x.GetName() == name);
}