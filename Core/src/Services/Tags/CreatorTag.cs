namespace Markwardt;

public abstract class CreatorTag<TCreator> : ServiceTag
    where TCreator : IServiceCreator
{
    protected override sealed object GetService(IServiceProvider services)
        => services.Create<TCreator>().Create();
}