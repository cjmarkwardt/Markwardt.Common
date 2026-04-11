namespace Markwardt.ElectronBridgeBuilder;

public static class Http
{
    public static async ValueTask Download(string source, string destination)
    {
        using HttpClient client = new();
        using Stream s = await client.GetStreamAsync(source);
        using FileStream fs = new(destination, FileMode.OpenOrCreate);
        await s.CopyToAsync(fs);
    }
}