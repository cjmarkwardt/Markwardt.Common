namespace Markwardt;

public static class ServiceSetup
{
    public static IEnumerable<object?> ResolveArguments(IEnumerable<ParameterInfo> parameters, IServiceProvider services, IReadOnlyDictionary<string, object?>? arguments = null)
        => parameters.Select(parameter =>
        {
            if (arguments is not null && arguments.TryGetValue(parameter.Name.NotNull(), out object? argument))
            {
                return argument;
            }

            object? service = services.GetService(parameter.GetCustomAttribute<InjectAttribute>()?.Service ?? parameter.ParameterType);
            if (service is not null)
            {
                return service;
            }
            else if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            throw new InvalidOperationException($"Unable to resolve parameter {parameter}");
        });

    public static MethodBase FindCreationMethod(Type implementationType)
    {
        bool IsCopyConstructor(ConstructorInfo constructor)
            => constructor.GetParameters().Length == 1 && constructor.GetParameters()[0].ParameterType == implementationType;

        IEnumerable<MethodBase> methods = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Where(c => !IsCopyConstructor(c)).Cast<MethodBase>().Concat(implementationType.GetMethods(BindingFlags.Static | BindingFlags.Public));

        MethodBase? method = methods.FirstOrDefault(m => m.GetCustomAttribute<FactoryAttribute>() is not null);
        if (method is null)
        {
            method = methods.OfType<ConstructorInfo>().OrderByDescending(m => m.GetParameters().Length).FirstOrDefault();
            if (method is null)
            {
                throw new InvalidOperationException($"No creation method found for implementation type {implementationType}");
            }
        }

        return method;
    }

    public static IEnumerable<(MemberInfo Member, Type ServiceType)> FindInjectableMembers(Type implementationType)
    {
        foreach (MemberInfo member in implementationType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>().Concat(implementationType.GetFields(BindingFlags.Instance | BindingFlags.Public)))
        {
            InjectAttribute? attribute = member.GetCustomAttribute<InjectAttribute>();
            if (attribute is not null)
            {
                yield return (member, attribute.Service ?? member.GetStorageType());
            }
            else if (member is PropertyInfo property && property.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)) == true)
            {
                yield return (member, member.GetStorageType());
            }
        }
    }
}