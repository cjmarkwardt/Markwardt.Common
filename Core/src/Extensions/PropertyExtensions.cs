namespace Markwardt;

public static class PropertyExtensions
{
    public static bool IsRequired(this PropertyInfo property)
        => Attribute.IsDefined(property, typeof(RequiredMemberAttribute));

    public static bool IsInit(this PropertyInfo property)
        => property.SetMethod is not null && property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));
}