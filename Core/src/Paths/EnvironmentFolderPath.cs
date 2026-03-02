namespace Markwardt;

[ServiceType<string>]
public abstract class EnvironmentFolderPathTag : ServiceTag
{
    protected abstract Environment.SpecialFolder Folder { get; }

    protected sealed override object Resolve(IServiceProvider services)
        => Environment.GetFolderPath(Folder);
}