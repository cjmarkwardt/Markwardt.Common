namespace Markwardt;

public abstract class ValueTag<T> : IServiceTag
{
    protected virtual Maybe<T> DefaultValue => default;

    public IService GetService()
        => new Service(() => DefaultValue.HasValue ? new SourceValue<T>(DefaultValue.Value) : new SourceValue<T>());
}