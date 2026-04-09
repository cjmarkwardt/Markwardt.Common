namespace Markwardt;

public abstract class GodotAssetTag<T>(string path) : ServiceTag
    where T : Resource
{
    protected override object Resolve(IServiceProvider services)
        => new GodotAssetx<T>(path);
}