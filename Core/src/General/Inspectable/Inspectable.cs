namespace Markwardt;

public interface IInspectable
{
    IDictionary<IInspectKey, object> Inspections { get; }
}

public static class InspectableExtensions
{
    public static IDictionary<IInspectKey, object> ChainInspections(this IDictionary<IInspectKey, object> inspections, object target)
        => inspections.WithFallback(Inspectable.GetInspections(target));

    public static Maybe<T> Inspect<T>(this object? target, InspectKey<T> key)
        where T : class
    {
        if (target is null)
        {
            return default;
        }
        else if (Inspectable.GetInspections(target).TryGetValue(key, out object? value))
        {
            return ((T)value).Maybe();
        }
        else
        {
            return default;
        }
    }

    public static Maybe<T> Inspect<T>(this object? target, InspectValueKey<T> key)
        where T : struct
        => target.Inspect((InspectKey<ValueWrapper<T>>)key).TryGetValue(out ValueWrapper<T>? wrapper) ? wrapper.Value.Maybe() : key.DefaultValue;

    public static TInspectable SetInspect<TInspectable, T>(this TInspectable target, InspectKey<T> key, T value)
        where TInspectable : class
        where T : class
    {
        Inspectable.GetInspections(target)[key] = value;
        return target;
    }

    public static TInspectable SetInspect<TInspectable, T>(this TInspectable target, InspectValueKey<T> key, T value)
        where TInspectable : class
        where T : struct
    {
        ValueWrapper<T> wrapper = target.Inspect((InspectKey<ValueWrapper<T>>)key).ValueOr(new());
        wrapper.Value = value;
        return target.SetInspect(key, wrapper);
    }
    
    public static TInspectable UnsetInspect<TInspectable, T>(this TInspectable target, InspectKey<T> key)
        where TInspectable : class
        where T : class
    {
        Inspectable.GetInspections(target).Remove(key);
        return target;
    }

    public static TInspectable CopyInspects<TInspectable, TSource>(this TInspectable target, TSource source)
        where TInspectable : class
        where TSource : class
    {
        IDictionary<IInspectKey, object> inspector = Inspectable.GetInspections(target);
        Inspectable.GetInspections(source).ForEach(x => inspector[x.Key] = x.Value);
        return target;
    }
}

public class Inspectable : IInspectable
{
    private static readonly ConditionalWeakTable<object, Dictionary<IInspectKey, object>> inspectors = [];

    public static IDictionary<IInspectKey, object> GetInspections(object target)
        => target is IInspectable inspectable ? inspectable.Inspections : inspectors.GetOrCreateValue(target);

    public IDictionary<IInspectKey, object> Inspections { get; private set; } = new Dictionary<IInspectKey, object>();

    public void ChainInspections(object target)
        => Inspections = Inspections.ChainInspections(target);
}

public class ControlledInspectable : Inspectable
{
    public ControlledInspectable()
        => ChainInspections(Controlled);

    public IInspectable Controlled { get; } = new Inspectable();
}