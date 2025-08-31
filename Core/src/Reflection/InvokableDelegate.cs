namespace Markwardt;

public class InvokableDelegate(InvokableDelegate.Implementation implementation) : IInvokable
{
    public delegate object? Implementation(object? instance, Span<object?> arguments);

    public object? Source => implementation;
    public IReadOnlyList<IInvokable.Parameter> Parameters => [];

    public object? Invoke(object? instance, Span<object?> arguments)
        => implementation(instance, arguments);
}