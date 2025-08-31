namespace Markwardt;

public abstract class GodotAssetTag<T>(string path) : ServiceTag
    where T : Resource
{
    protected override sealed object GetService(IServiceProvider services)
        => new GodotAssetx<T>(path);
}