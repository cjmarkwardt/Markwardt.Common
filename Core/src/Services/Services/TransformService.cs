namespace Markwardt;

public class TransformService<T>(Func<T, object?> transform, Type? tag = null) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value = services.GetService(tag ?? typeof(T));
        return value is null ? null : transform((T)value);
    }

    public override string ToString()
        => $"{base.ToString()} (Tag: {tag ?? typeof(T)})";
}

public class TransformService<T1, T2>(Func<T1, T2, object?> transform, Type? tag1 = null, Type? tag2 = null) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value1 = services.GetService(tag1 ?? typeof(T1));
        object? value2 = services.GetService(tag2 ?? typeof(T2));
        return value1 is null || value2 is null ? null : transform((T1)value1, (T2)value2);
    }

    public override string ToString()
        => $"{base.ToString()} (Tag1: {tag1 ?? typeof(T1)}, Tag2: {tag2 ?? typeof(T2)})";
}

public class TransformService<T1, T2, T3>(Func<T1, T2, T3, object?> transform, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value1 = services.GetService(tag1 ?? typeof(T1));
        object? value2 = services.GetService(tag2 ?? typeof(T2));
        object? value3 = services.GetService(tag3 ?? typeof(T3));
        return value1 is null || value2 is null || value3 is null ? null : transform((T1)value1, (T2)value2, (T3)value3);
    }

    public override string ToString()
        => $"{base.ToString()} (Tag1: {tag1 ?? typeof(T1)}, Tag2: {tag2 ?? typeof(T2)}, Tag3: {tag3 ?? typeof(T3)})";
}

public class TransformService<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object?> transform, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value1 = services.GetService(tag1 ?? typeof(T1));
        object? value2 = services.GetService(tag2 ?? typeof(T2));
        object? value3 = services.GetService(tag3 ?? typeof(T3));
        object? value4 = services.GetService(tag4 ?? typeof(T4));
        return value1 is null || value2 is null || value3 is null || value4 is null ? null : transform((T1)value1, (T2)value2, (T3)value3, (T4)value4);
    }

    public override string ToString()
        => $"{base.ToString()} (Tag1: {tag1 ?? typeof(T1)}, Tag2: {tag2 ?? typeof(T2)}, Tag3: {tag3 ?? typeof(T3)}, Tag4: {tag4 ?? typeof(T4)})";
}

public class TransformService<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object?> transform, Type? tag1 = null, Type? tag2 = null, Type? tag3 = null, Type? tag4 = null, Type? tag5 = null) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
    {
        object? value1 = services.GetService(tag1 ?? typeof(T1));
        object? value2 = services.GetService(tag2 ?? typeof(T2));
        object? value3 = services.GetService(tag3 ?? typeof(T3));
        object? value4 = services.GetService(tag4 ?? typeof(T4));
        object? value5 = services.GetService(tag5 ?? typeof(T5));
        return value1 is null || value2 is null || value3 is null || value4 is null || value5 is null ? null : transform((T1)value1, (T2)value2, (T3)value3, (T4)value4, (T5)value5);
    }

    public override string ToString()
        => $"{base.ToString()} (Tag1: {tag1 ?? typeof(T1)}, Tag2: {tag2 ?? typeof(T2)}, Tag3: {tag3 ?? typeof(T3)}, Tag4: {tag4 ?? typeof(T4)}, Tag5: {tag5 ?? typeof(T5)})";
}