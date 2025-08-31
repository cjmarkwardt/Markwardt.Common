namespace Markwardt;

public static class DefaultFactoryExtensions
{
    private static readonly Dictionary<Type, IInvokable?> defaultFactories = [];

    public static IInvokable? GetDefaultFactory(this Type type)
        => defaultFactories.GetOrAdd(type, () =>
        {
            Type? implementation = null;
            if (type.GetCustomAttribute<DefaultImplementationAttribute>().TryNotNull(out DefaultImplementationAttribute attribute))
            {
                implementation = attribute.Implementation;
            }
            else if (type.IsClass && !type.IsAbstract)
            {
                implementation = type;
            }
            else if (type.IsInterface && type.Name.StartsWith('I'))
            {
                string name = type.Name[1..];
                if (type.Namespace is not null)
                {
                    name = $"{type.Namespace}.{name}";
                }

                implementation = type.Assembly.GetType(name);
            }

            if (implementation is null)
            {
                return null;
            }

            bool IsCopyConstructor(ConstructorInfo constructor)
                => constructor.GetParameters().Length == 1 && constructor.GetParameters()[0].ParameterType == implementation;

            IEnumerable<MethodBase> methods = implementation.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Where(c => !IsCopyConstructor(c)).Cast<MethodBase>().Concat(implementation.GetMethods(BindingFlags.Static | BindingFlags.Public));

            MethodBase? method = methods.FirstOrDefault(m => m.GetCustomAttribute<FactoryAttribute>() is not null);
            method ??= methods.OfType<ConstructorInfo>().OrderByDescending(m => m.GetParameters().Length).FirstOrDefault();

            return method is null ? null : new InvokableMethod(() => method);
        });
}