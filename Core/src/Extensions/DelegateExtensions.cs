namespace Markwardt;

public static class DelegateExtensions
{
    public static Type? GetDelegateResult(this Type type)
    {
        MethodInfo? method = type.GetMethod(nameof(Action.Invoke));
        if (method is not null)
        {
            return method.ReturnType;
        }

        return null;
    }

    public static bool IsDelegate(this Type type)
        => type.GetDelegateResult() is not null;
}