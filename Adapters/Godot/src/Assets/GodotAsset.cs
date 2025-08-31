namespace Markwardt;

public class GodotAssetx<T>(string path) : Asset<T>
    where T : Resource
{
    protected override async Task<T> ExecuteLoad()
        => await Task.Run(() => GD.Load<T>(path));

    protected override void ExecuteUnload(T value)
        => value.Dispose();
}