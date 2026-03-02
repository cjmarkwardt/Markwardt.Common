namespace Markwardt;

public abstract class CreatorTag<TCreator> : ServiceTag
    where TCreator : IServiceCreator
{
    protected override sealed object Resolve(IServiceProvider services)
        => services.Create<TCreator>().Create();
}