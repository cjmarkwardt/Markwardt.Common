namespace Markwardt;

[ServiceType<string>]
public abstract class EnvironmentFolderPathTag : ServiceTag
{
    protected abstract Environment.SpecialFolder Folder { get; }

    protected override object GetService(IServiceProvider services)
        => Environment.GetFolderPath(Folder);
}