namespace Markwardt;

public static class MemberExtensions
{
    public static Type GetStorageType(this MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            return property.PropertyType;
        }
        else if (member is FieldInfo field)
        {
            return field.FieldType;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static void SetValue(this MemberInfo member, object? instance, object? value)
    {
        if (member is PropertyInfo property)
        {
            property.SetValue(instance, value);
        }
        else if (member is FieldInfo field)
        {
            field.SetValue(instance, value);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static object? GetValue(this MemberInfo member, object? instance)
    {
        if (member is PropertyInfo property)
        {
            return property.GetValue(instance);
        }
        else if (member is FieldInfo field)
        {
            return field.GetValue(instance);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}