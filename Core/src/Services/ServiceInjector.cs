namespace Markwardt;

public interface IServiceInjector
{
    object Inject(IServiceProvider services, object instance);
}

public class ServiceInjector : IServiceInjector
{
    public static IServiceInjector Shared { get; } = new ServiceInjector();

    private readonly Dictionary<Type, TypedInjector> injectors = [];

    public object Inject(IServiceProvider services, object instance)
    {
        Type type = instance.GetType();
        if (!injectors.TryGetValue(type, out TypedInjector? injector))
        {
            injector = new(type);
            injectors.Add(type, injector);
        }

        injector.Inject(services, instance);
        return instance;
    }

    private sealed class TypedInjector
    {
        private delegate void Setter(object instance, object? value);

        public TypedInjector(Type type)
        {
            foreach ((MemberInfo member, Type? serviceType) in ServiceSetup.FindInjectableMembers(type))
            {
                if (member is PropertyInfo property)
                {
                    injectors.Add(new(CreateSetter(property.PropertyType, instance => Expression.Property(Expression.Convert(instance, type), property)), serviceType));
                }
                else if (member is FieldInfo field)
                {
                    injectors.Add(new(CreateSetter(field.FieldType, instance => Expression.Field(Expression.Convert(instance, type), field)), serviceType));
                }
            }
        }

        private readonly List<MemberInjector> injectors = [];

        public void Inject(IServiceProvider services, object instance)
        {
            foreach (MemberInjector injector in injectors)
            {
                injector.Inject(services, instance);
            }
        }

        private Setter CreateSetter(Type valueType, Func<ParameterExpression, MemberExpression> getTarget)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object));
            ParameterExpression valueParameter = Expression.Parameter(typeof(object));
            return Expression.Lambda<Setter>(Expression.Assign(getTarget(instanceParameter), Expression.Convert(valueParameter, valueType)), instanceParameter, valueParameter).Compile();
        }

        private sealed class MemberInjector(Setter setter, Type serviceType)
        {
            public void Inject(IServiceProvider services, object instance)
                => setter(instance, services.GetRequiredService(serviceType));
        }
    }
}