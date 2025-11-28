namespace Markwardt;

public class ConstructorService(InvokableMethod constructor, IReadOnlyDictionary<ParameterInfo, IService>? parameterBuilders = null, IReadOnlyDictionary<PropertyInfo, IService>? propertyBuilders = null) : BaseAsyncDisposable, IService
{
    private readonly IReadOnlyList<ParameterResolver> parameterResolvers = constructor.Parameters.Select(x => new ParameterResolver(x)).ToList();
    private readonly IReadOnlyList<PropertyInjector> propertyInjectors = constructor.ResultType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.SetMethod is not null).Select(x => new PropertyInjector(x)).ToList();

    public async ValueTask<object> Resolve(IAsyncServiceProvider services, IReadOnlyDictionary<ParameterInfo, object?>? parameters = null, IReadOnlyDictionary<PropertyInfo, object?>? properties = null, CancellationToken cancellation = default)
    {
        object?[] arguments = new object?[parameterResolvers.Count];
        for (int i = 0; i < parameterResolvers.Count; i++)
        {
            arguments[i] = (await parameterResolvers[i].Resolve(services, parameterBuilders, parameters, cancellation)).Value;
        }

        object instance = (await constructor.Invoke(null, arguments, cancellation)).NotNull();

        foreach (PropertyInjector property in propertyInjectors)
        {
            await property.Inject(services, instance, propertyBuilders, properties, cancellation);
        }

        if (instance is IAsyncInitializable asyncInitializable)
        {
            await asyncInitializable.Initialize(cancellation);
        }

        return instance;
    }

    private enum ResolveMode
    {
        Skip,
        Optional,
        Required
    }

    private abstract class Resolver<T>
        where T : ICustomAttributeProvider
    {
        public Resolver(T target)
        {
            Target = target;
            Service = Target.GetCustomAttribute<ServiceAttribute>()?.GetService(Type);
        }

        public T Target { get; }
        public IService? Service { get; }

        public abstract Type Type { get; }
        public abstract Maybe<object?> DefaultValue { get; }
        public abstract ResolveMode Mode { get; }

        public async ValueTask<Maybe<object?>> Resolve(IAsyncServiceProvider services, IReadOnlyDictionary<T, IService>? serviceOverrides, IReadOnlyDictionary<T, object?>? values, CancellationToken cancellation)
        {
            if (values is not null && values.TryGetValue(Target, out object? argument))
            {
                return argument.Maybe();
            }
            else if (serviceOverrides is not null && serviceOverrides.TryGetValue(Target, out IService? service))
            {
                return (await service.Resolve(services, null, null, cancellation)).Maybe<object?>();
            }
            else if (Service is not null)
            {
                return (await Service.Resolve(services, null, null, cancellation)).Maybe<object?>();
            }
            else if (Mode is ResolveMode.Skip)
            {
                return DefaultValue;
            }
            else
            {
                return await GetService(services, Type.GetDefaultImplementation(), cancellation);
            }
        }

        private async ValueTask<Maybe<object?>> GetService(IAsyncServiceProvider services, Type type, CancellationToken cancellation)
        {
            object? service = await services.GetService(type, cancellation);
            if (service is null && Mode is ResolveMode.Required)
            {
                throw new InvalidOperationException($"Service {type} is required but could not be resolved");
            }

            return service.Maybe();
        }
    }

    private sealed class ParameterResolver(ParameterInfo target) : Resolver<ParameterInfo>(target)
    {
        public override Type Type => Target.ParameterType;
        public override Maybe<object?> DefaultValue => Target.HasDefaultValue ? Target.DefaultValue.Maybe() : default;

        public override ResolveMode Mode { get; } = !target.IsNullable() || !target.HasDefaultValue ? ResolveMode.Required : ResolveMode.Optional;
    }

    private sealed class PropertyInjector(PropertyInfo target) : Resolver<PropertyInfo>(target)
    {
        private readonly MethodInvoker setter = MethodInvoker.Create(target.SetMethod.NotNull());

        public override Type Type => Target.PropertyType;
        public override Maybe<object?> DefaultValue => default;

        public override ResolveMode Mode { get; } = !target.IsInit() && !target.IsRequired() ? ResolveMode.Skip : target.IsNullable() ? ResolveMode.Optional : ResolveMode.Required;

        public async Task Inject(IAsyncServiceProvider services, object instance, IReadOnlyDictionary<PropertyInfo, IService>? builders, IReadOnlyDictionary<PropertyInfo, object?>? values, CancellationToken cancellation)
        {
            Maybe<object?> value = await Resolve(services, builders, values, cancellation);
            if (value.HasValue)
            {
                setter.Invoke(instance, value.Value);
            }
        }
    }
}