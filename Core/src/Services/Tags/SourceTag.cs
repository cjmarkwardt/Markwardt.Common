namespace Markwardt;

public abstract class SourceTag<TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed object GetService(IServiceProvider services)
        => GetService(services, services.GetRequiredService<TSource>());

    protected abstract object GetService(IServiceProvider services, TSource source);
}

public abstract class SourceTag<TSourceTag, TSource> : ServiceTag
    where TSource : notnull
{
    protected override sealed object GetService(IServiceProvider services)
        => GetService(services, services.GetRequiredService<TSourceTag, TSource>());

    protected abstract object GetService(IServiceProvider services, TSource source);
}