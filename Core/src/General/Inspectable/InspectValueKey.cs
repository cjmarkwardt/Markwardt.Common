namespace Markwardt;

public class InspectValueKey<T>(string name, Maybe<T> defaultValue = default) : InspectKey<ValueWrapper<T>>(name)
    where T : struct
{
    public Maybe<T> DefaultValue => defaultValue;
}