namespace Markwardt;

public static class TypeExtensions
{
    public static Type GetResultType(this Type type)
    {
        if (type == typeof(Task) || type == typeof(ValueTask))
        {
            return typeof(void);
        }
        else if (type.IsGenericType)
        {
            Type genericDefinition = type.GetGenericTypeDefinition();
            if (genericDefinition == typeof(Task<>) || genericDefinition == typeof(ValueTask<>))
            {
                return type.GetGenericArguments()[0];
            }
        }

        return type;
    }
}