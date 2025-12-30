namespace Markwardt;

public interface IService : IDisposable, IAsyncDisposable
{
    object Resolve(IServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null);
}

public static class ServiceExtensions
{
    public static T Resolve<T>(this IService service, IServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null)
        => (T) service.Resolve(services, parameters, properties);
}

public class Service(Func<IServiceProvider, IReadOnlyDictionary<ParameterInfo, object?>?, IReadOnlyDictionary<PropertyInfo, object?>?, object> resolve) : BaseAsyncDisposable, IService
{
    public static IService Delegate(Func<IServiceProvider, IReadOnlyDictionary<ParameterInfo, object?>?, IReadOnlyDictionary<PropertyInfo, object?>?, object> resolve, bool isCached = true)
        => new Service(resolve).AsCached(isCached);

    public static IService Delegate(Func<IServiceProvider, object> resolve, bool isCached = true)
        => Delegate((services, _, _) => resolve(services), isCached);

    public static IService Delegate(Func<object> resolve, bool isCached = true)
        => Delegate(_ => resolve(), isCached);

    public static IService Instance(object instance, bool isCached = true)
        => Delegate(_ => instance, isCached);

    public static IService Constructor(InvokableMethod constructor, Action<IServiceConfigurator>? configure = null, bool isCached = true)
    {
        ServiceConfigurator builder = new(constructor);
        configure?.Invoke(builder);
        return builder.Build().AsCached(isCached);
    }

    public static IService Constructor(MethodBase constructor, Action<IServiceConfigurator>? configure = null, bool isCached = true)
        => Constructor(new InvokableMethod(constructor), configure, isCached);

    public static IService Constructor(Type type, string? constructorName, Action<IServiceConfigurator>? configure = null, bool isCached = true)
        => Constructor(new InvokableMethod(type.FindConstructor(constructorName).NotNull()), configure, isCached);

    public static IService Constructor(Type type, Action<IServiceConfigurator>? configure = null, bool isCached = true)
        => Constructor(type, null, configure, isCached);

    public static IService Constructor<T>(string? constructorName, Action<IServiceConfigurator>? configure = null, bool isCached = true)
        where T : notnull
        => Constructor(typeof(T), constructorName, configure, isCached);

    public static IService Constructor<T>(Action<IServiceConfigurator>? configure = null, bool isCached = true)
        where T : notnull
        => Constructor(typeof(T), configure, isCached);

    public static IService Factory(Type factory, InvokableMethod constructor, Action<IServiceConfigurator>? configure = null, IReadOnlyDictionary<ParameterInfo, ParameterInfo>? parameterMappings = null, bool isCached = true)
        => Delegate(services =>
        {
            if (constructor.ResultType == typeof(void))
            {
                throw new InvalidOperationException($"Constructor must have a result type");
            }

            MethodInfo factoryInvocation = factory.GetDelegateInvocation() ?? throw new InvalidOperationException($"Factory type {factory} is not a delegate");

            Type factoryResult = factoryInvocation.ReturnType.GetResultType();
            if (!constructor.ResultType.IsAssignableTo(factoryResult))
            {
                throw new InvalidOperationException($"Constructor result type {constructor.ResultType} cannot be assigned to factory result type {factoryResult}");
            }

            Dictionary<ParameterInfo, ParameterInfo> resolvedParameters = parameterMappings?.ToDictionary() ?? [];
            foreach (ParameterInfo unresolvedParameter in factoryInvocation.GetParameters().Where(x => !resolvedParameters.ContainsKey(x)))
            {
                resolvedParameters[unresolvedParameter] = constructor.Parameters.FirstOrDefault(x => x.Name == unresolvedParameter.Name) ?? throw new InvalidOperationException($"No matching constructor parameter found for factory parameter {unresolvedParameter}");
            }

            IService service = Constructor(constructor, configure, false);
            return Delegator.CreateDelegate(factory, arguments => service.Resolve(services, arguments.ToDictionary(x => resolvedParameters[x.Key], x => x.Value), null));
        }, isCached);

    public static IService Factory(Type factory, MethodBase constructor, Action<IServiceConfigurator>? configure = null, IReadOnlyDictionary<ParameterInfo, ParameterInfo>? parameterMappings = null, bool isCached = true)
        => Factory(factory, new InvokableMethod(constructor), configure, parameterMappings, isCached);

    public static IService Factory(Type factory, Type? type = null, string? constructorName = null, Action<IServiceConfigurator>? configure = null, IReadOnlyDictionary<ParameterInfo, ParameterInfo>? parameterMappings = null, bool isCached = true)
    {
        type ??= factory.GetDelegateResult() ?? throw new InvalidOperationException($"Factory type {factory} must be a delegate type");
        return Factory(factory, type?.FindConstructor(constructorName) ?? throw new InvalidOperationException($"Type {type} must have a default constructor"), configure, parameterMappings, isCached);
    }

    public static IService Factory<TFactory>(Type? type = null, string? constructorName = null, Action<IServiceConfigurator>? configure = null, IReadOnlyDictionary<ParameterInfo, ParameterInfo>? parameterMappings = null, bool isCached = true)
        where TFactory : notnull, Delegate
        => Factory(typeof(TFactory), type, constructorName, configure, parameterMappings, isCached);

    public static IService Factory<TFactory, T>(string? constructorName = null, Action<IServiceConfigurator>? configure = null, IReadOnlyDictionary<ParameterInfo, ParameterInfo>? parameterMappings = null, bool isCached = true)
        where TFactory : notnull, Delegate
        where T : notnull
        => Factory(typeof(TFactory), typeof(T), constructorName, configure, parameterMappings, isCached);

    public static IService Route(Type tag, bool isCached = false)
        => Delegate(services => services.GetRequiredService(tag), isCached);

    public static IService Route<TTag>(bool isCached = false)
        where TTag : notnull
        => Delegate(services => services.GetRequiredService<TTag>(), isCached);

    public static IService Source<T>(Func<T, object> resolve, Type? tag1 = null, bool isCached = false)
        where T : notnull
        => Delegate(services => resolve((T) services.GetRequiredService(tag1 ?? typeof(T))), isCached);

    public static IService Source<T1, T2>(Func<T1, T2, object> resolve, Type? tag1 = null, Type? tag2 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        => Delegate(services => resolve((T1) services.GetRequiredService(tag1 ?? typeof(T1)), (T2) services.GetRequiredService(tag2 ?? typeof(T2))), isCached);

    public static IService Source<T1, T2, T3>(Func<T1, T2, T3, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        => Delegate(services => resolve((T1) services.GetRequiredService(tag1 ?? typeof(T1)), (T2) services.GetRequiredService(tag2 ?? typeof(T2)), (T3) services.GetRequiredService(tag3 ?? typeof(T3))), isCached);

    public static IService Source<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        => Delegate(services => resolve((T1) services.GetRequiredService(tag1 ?? typeof(T1)), (T2) services.GetRequiredService(tag2 ?? typeof(T2)), (T3) services.GetRequiredService(tag3 ?? typeof(T3)), (T4) services.GetRequiredService(tag4 ?? typeof(T4))), isCached);

    public static IService Source<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null, Type? tag5 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        => Delegate(services => resolve((T1) services.GetRequiredService(tag1 ?? typeof(T1)), (T2) services.GetRequiredService(tag2 ?? typeof(T2)), (T3) services.GetRequiredService(tag3 ?? typeof(T3)), (T4) services.GetRequiredService(tag4 ?? typeof(T4)), (T5) services.GetRequiredService(tag5 ?? typeof(T5))), isCached);

    public object Resolve(IServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null)
        => resolve(services, parameters, properties);
}