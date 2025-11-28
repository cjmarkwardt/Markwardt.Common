namespace Markwardt;

[ServiceType<string>]
public abstract class EnvironmentFolderPathTag : SimpleTag
{
    protected abstract Environment.SpecialFolder Folder { get; }

    protected override object Get()
        => Environment.GetFolderPath(Folder);
}