namespace Markwardt;

public static class DelegateExtensions
{
    public static MethodInfo? GetDelegateInvocation(this Type type)
    {
        MethodInfo? method = type.GetMethod(nameof(Action.Invoke));
        if (method is not null)
        {
            return method;
        }

        return null;
    }

    public static Type? GetDelegateResult(this Type type)
        => type.GetDelegateInvocation()?.ReturnType;

    public static bool IsDelegate(this Type type)
        => type.GetDelegateInvocation() is not null;
}