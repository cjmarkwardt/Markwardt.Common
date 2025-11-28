namespace Markwardt;

public abstract class GodotAssetTag<T>(string path) : SimpleTag
    where T : Resource
{
    protected override sealed object Get()
        => new GodotAssetx<T>(path);
}