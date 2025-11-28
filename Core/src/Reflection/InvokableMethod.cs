namespace Markwardt;

public class InvokableMethod : ICustomAttributeProvider
{
    public InvokableMethod(MethodBase methodBase)
    {
        this.methodBase = methodBase;

        List<ParameterInfo> parameters = methodBase.GetParameters().ToList();
        if (parameters.LastOrDefault()?.ParameterType == typeof(CancellationToken))
        {
            parameters.RemoveAt(parameters.Count - 1);
            hasCancellation = true;
        }

        Parameters = parameters;

        if (methodBase is ConstructorInfo constructor)
        {
            constructorInvoker = ConstructorInvoker.Create(constructor);
            InstanceType = null;
            ResultType = constructor.DeclaringType.NotNull();
        }
        else if (methodBase is MethodInfo method)
        {
            methodInvoker = MethodInvoker.Create(method);
            InstanceType = method.IsStatic ? null : method.DeclaringType.NotNull();

            ResultType = method.ReturnType.GetResultType();
            ResultAttributes = method.ReturnTypeCustomAttributes;
        }
        else
        {
            throw new NotSupportedException($"Method type {methodBase} is not supported");
        }
    }

    private readonly MethodBase methodBase;
    private readonly ConstructorInvoker? constructorInvoker;
    private readonly MethodInvoker? methodInvoker;
    private readonly bool hasCancellation;

    public Type? InstanceType { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public Type ResultType { get; }
    public ICustomAttributeProvider? ResultAttributes { get; }

    public object[] GetCustomAttributes(bool inherit)
        => methodBase.GetCustomAttributes(inherit);

    public object[] GetCustomAttributes(Type attributeType, bool inherit)
        => methodBase.GetCustomAttributes(attributeType, inherit);

    public bool IsDefined(Type attributeType, bool inherit)
        => methodBase.IsDefined(attributeType, inherit);

    public async ValueTask<object?> Invoke(object? instance, Memory<object?> inputs, CancellationToken cancellation = default)
    {
        if (InstanceType is null && instance is not null)
        {
            throw new InvalidOperationException($"Unexpected instance on invocation");
        }
        else if (InstanceType is not null && instance is null)
        {
            throw new InvalidOperationException($"Missing instance on invocation");
        }

        if (hasCancellation)
        {
            Memory<object?> newInputs = new object?[inputs.Length + 1];
            inputs.CopyTo(newInputs);
            newInputs.Span[newInputs.Length - 1] = cancellation;
            inputs = newInputs;
        }

        if (constructorInvoker is not null)
        {
            return constructorInvoker.Invoke(inputs.Span);
        }
        else if (methodInvoker is not null)
        {
            object? result = methodInvoker.Invoke(instance, inputs.Span);
            switch (result)
            {
                case Task task:
                    await task;
                    result = ResultType == typeof(void) ? null : ((dynamic)task).Result;
                    break;
                case ValueTask task:
                    await task;
                    result = ResultType == typeof(void) ? null : ((dynamic)task).Result;
                    break;
            }

            return result;
        }
        else
        {
            throw new InvalidOperationException($"No invoker available for {methodBase}");
        }
    }
}