namespace Markwardt;

public static class MethodExtensions
{
    public static object? SafeInvoke(this MethodBase method, object? instance = null, object?[]? arguments = null)
    {
        if (method is ConstructorInfo constructor && instance is null)
        {
            return constructor.Invoke(arguments);
        }
        else
        {
            return method.Invoke(instance, arguments);
        }
    }

    public static object? SafeInvoke(this MethodBase method, object? instance = null, IEnumerable<object?>? arguments = null)
        => method.SafeInvoke(instance, arguments?.ToArray());
}