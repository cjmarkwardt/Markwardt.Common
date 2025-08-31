namespace Markwardt;

[ServiceType<string>]
public class ApplicationDataRootFolderPathTag : EnvironmentFolderPathTag
{
    protected override Environment.SpecialFolder Folder => Environment.SpecialFolder.ApplicationData;
}