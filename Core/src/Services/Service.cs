namespace Markwardt;

public static class ServiceExtensions
{
    public static async ValueTask<T> Resolve<T>(this IService service, IAsyncServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, CancellationToken cancellation = default)
        => (T)await service.Resolve(services, parameters, properties, cancellation);
}

public interface IService : IDisposable, IAsyncDisposable
{
    ValueTask<object> Resolve(IAsyncServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, CancellationToken cancellation = default);
}

public class Service(AsyncFunction<IAsyncServiceProvider, IReadOnlyDictionary<ParameterInfo, object?>?, IReadOnlyDictionary<PropertyInfo, object?>?, object> resolve) : BaseAsyncDisposable, IService
{
    public static IService Delegate(AsyncFunction<IAsyncServiceProvider, IReadOnlyDictionary<ParameterInfo, object?>?, IReadOnlyDictionary<PropertyInfo, object?>?, object> resolve, bool isCached = true)
        => new Service(resolve).AsCached(isCached);

    public static IService Delegate(AsyncFunction<IAsyncServiceProvider, object> resolve, bool isCached = true)
        => Delegate(async (services, _, _, cancellation) => await resolve(services, cancellation), isCached);

    public static IService Delegate(AsyncFunction<object> resolve, bool isCached = true)
        => Delegate(async (_, cancellation) => await resolve(cancellation), isCached);

    public static IService Instance(object instance, bool isCached = true)
        => Delegate(_ => ValueTask.FromResult(instance), isCached);

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
        => Delegate((services, _) =>
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
        return ValueTask.FromResult<object>(Delegator.CreateDelegate(factory, (arguments, cancellation) => service.Resolve(services, arguments.ToDictionary(x => resolvedParameters[x.Key], x => x.Value), null, cancellation)));
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
        => Delegate(async (services, cancellation) => await services.GetRequiredService(tag, cancellation), isCached);

    public static IService Route<TTag>(bool isCached = false)
        where TTag : notnull
        => Delegate(async (services, cancellation) => await services.GetRequiredService<TTag>(cancellation), isCached);

    public static IService Source<T>(AsyncFunction<T, object> resolve, Type? tag1 = null, bool isCached = false)
        where T : notnull
        => Delegate(async (services, cancellation) => await resolve((T) await services.GetRequiredService(tag1 ?? typeof(T), cancellation), cancellation), isCached);

    public static IService Source<T1, T2>(AsyncFunction<T1, T2, object> resolve, Type? tag1 = null, Type? tag2 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        => Delegate(async (services, cancellation) => await resolve((T1) await services.GetRequiredService(tag1 ?? typeof(T1), cancellation), (T2) await services.GetRequiredService(tag2 ?? typeof(T2), cancellation), cancellation), isCached);

    public static IService Source<T1, T2, T3>(AsyncFunction<T1, T2, T3, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        => Delegate(async (services, cancellation) => await resolve((T1) await services.GetRequiredService(tag1 ?? typeof(T1), cancellation), (T2) await services.GetRequiredService(tag2 ?? typeof(T2), cancellation), (T3) await services.GetRequiredService(tag3 ?? typeof(T3), cancellation), cancellation), isCached);

    public static IService Source<T1, T2, T3, T4>(AsyncFunction<T1, T2, T3, T4, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        => Delegate(async (services, cancellation) => await resolve((T1) await services.GetRequiredService(tag1 ?? typeof(T1), cancellation), (T2) await services.GetRequiredService(tag2 ?? typeof(T2), cancellation), (T3) await services.GetRequiredService(tag3 ?? typeof(T3), cancellation), (T4) await services.GetRequiredService(tag4 ?? typeof(T4), cancellation), cancellation), isCached);

    public static IService Source<T1, T2, T3, T4, T5>(AsyncFunction<T1, T2, T3, T4, T5, object> resolve, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null, Type? tag5 = null, bool isCached = false)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        => Delegate(async (services, cancellation) => await resolve((T1) await services.GetRequiredService(tag1 ?? typeof(T1), cancellation), (T2) await services.GetRequiredService(tag2 ?? typeof(T2), cancellation), (T3) await services.GetRequiredService(tag3 ?? typeof(T3), cancellation), (T4) await services.GetRequiredService(tag4 ?? typeof(T4), cancellation), (T5) await services.GetRequiredService(tag5 ?? typeof(T5), cancellation), cancellation), isCached);

    public async ValueTask<object> Resolve(IAsyncServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, CancellationToken cancellation = default)
        => await resolve(services, parameters, properties, cancellation);
}