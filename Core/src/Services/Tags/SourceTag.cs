namespace Markwardt;

public abstract class SourceTag<TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed object GetService(IServiceProvider services)
        => Get(services, services.GetRequiredService<TSource>());

    protected abstract object Get(IServiceProvider services, TSource source);
}

public abstract class SourceTag<TSourceTag, TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed object GetService(IServiceProvider services)
        => Get(services, services.GetRequiredService<TSourceTag, TSource>());

    protected abstract object Get(IServiceProvider services, TSource source);
}