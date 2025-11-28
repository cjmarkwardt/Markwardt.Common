namespace Markwardt;

public static class AttributeExtensions
{
    public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this ICustomAttributeProvider provider, bool inherit = true)
        where TAttribute : Attribute
        => provider.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>();

    public static TAttribute? GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit = true)
        where TAttribute : Attribute
        => provider.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();

    public static bool IsDefined<TAttribute>(this ICustomAttributeProvider provider, bool inherit = true)
        where TAttribute : Attribute
        => provider.IsDefined(typeof(TAttribute), inherit);
}