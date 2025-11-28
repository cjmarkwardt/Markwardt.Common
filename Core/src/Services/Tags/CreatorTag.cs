namespace Markwardt;

public abstract class CreatorTag<TCreator> : ServiceTag
    where TCreator : IServiceCreator
{
    protected override sealed async ValueTask<object> GetService(IAsyncServiceProvider services, CancellationToken cancellation = default)
        => (await Service.Constructor<TCreator>().Resolve<TCreator>(services, cancellation: cancellation)).Create();
}