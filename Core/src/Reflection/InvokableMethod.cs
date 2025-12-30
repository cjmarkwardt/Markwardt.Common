namespace Markwardt;

public class InvokableMethod : ICustomAttributeProvider
{
    public InvokableMethod(MethodBase methodBase)
    {
        this.methodBase = methodBase;

        Parameters = methodBase.GetParameters().ToList();

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

    public object? Invoke(object? instance, Memory<object?> inputs)
    {
        if (InstanceType is null && instance is not null)
        {
            throw new InvalidOperationException($"Unexpected instance on invocation");
        }
        else if (InstanceType is not null && instance is null)
        {
            throw new InvalidOperationException($"Missing instance on invocation");
        }

        if (constructorInvoker is not null)
        {
            try
            {
                return constructorInvoker.Invoke(inputs.Span);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to invoke constructor {methodBase} on type {methodBase.DeclaringType}", exception);
            }
        }
        else if (methodInvoker is not null)
        {
            try
            {
                return methodInvoker.Invoke(instance, inputs.Span);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to invoke method {methodBase} on type {methodBase.DeclaringType}", exception);
            }
        }
        else
        {
            throw new InvalidOperationException($"No invoker available for {methodBase}");
        }
    }
}