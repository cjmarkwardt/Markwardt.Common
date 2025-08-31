namespace Markwardt;

public class InvokableMethod : IInvokable
{
    public InvokableMethod(Func<MethodBase> getMethod)
    {
        source = new(getMethod);
        parameters = new(Source.GetParameters().Select(x => new IInvokable.Parameter(x)).ToList);
    }

    private ConstructorInvoker? constructorInvoker;
    private MethodInvoker? methodInvoker;

    private readonly Lazy<MethodBase> source;
    public MethodBase Source => source.Value;

    private readonly Lazy<IReadOnlyList<IInvokable.Parameter>> parameters;
    public IReadOnlyList<IInvokable.Parameter> Parameters => parameters.Value;

    object? IInvokable.Source => Source;

    public object? Invoke(object? instance, Span<object?> arguments)
    {
        if (constructorInvoker is null && methodInvoker is null)
        {
            if (Source is ConstructorInfo constructor)
            {
                constructorInvoker = ConstructorInvoker.Create(constructor);
            }
            else
            {
                methodInvoker = MethodInvoker.Create(Source);
            }
        }

        if (constructorInvoker is not null)
        {
            if (instance is not null)
            {
                throw new InvalidOperationException("Cannot invoke a constructor on an instance");
            }

            return constructorInvoker.Invoke(arguments);
        }
        else
        {
            return methodInvoker.NotNull().Invoke(instance, arguments);
        }
    }
}