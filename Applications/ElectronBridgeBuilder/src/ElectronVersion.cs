namespace Markwardt.ElectronBridgeBuilder;

public record ElectronVersion(string Version, string Platform)
{
    public string Id => $"electron-{Version}-{Platform}";
    public string RepositoryId => $"electron-v{Version}-{Platform.Replace("win", "win32")}";
}