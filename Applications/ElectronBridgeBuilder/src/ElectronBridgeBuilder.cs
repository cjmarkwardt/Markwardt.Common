namespace Markwardt.ElectronBridgeBuilder;

public class Builder(string bridgeFolder, string buildFolder, string outputFolder) : IDisposable
{
    public async ValueTask Build(ElectronVersion version)
    {
        Directory.CreateDirectory(outputFolder);

        string outputFile = Path.Combine(outputFolder, $"{version.Id}.zip");

        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        if (Directory.Exists(buildFolder))
        {
            Directory.Delete(buildFolder, true);
        }

        Directory.CreateDirectory(buildFolder);

        string electronFile = Path.Combine(buildFolder, $"{version.Id}.zip");
        await Http.Download($"https://github.com/electron/electron/releases/download/v{version.Version}/{version.RepositoryId}.zip", electronFile);

        string electronFolder = Path.Combine(buildFolder, version.Id);
        await ZipFile.ExtractToDirectoryAsync(electronFile, electronFolder);
        File.Delete(electronFile);

        string electronResourcesFolder = Path.Combine(electronFolder, "resources");
        string electronApplicationFolder = Path.Combine(electronResourcesFolder, "app");
        File.Delete(Path.Combine(electronResourcesFolder, "default_app.asar"));
        Directory.CreateDirectory(electronApplicationFolder);
        File.Copy(Path.Combine(bridgeFolder, "package.json"), Path.Combine(electronApplicationFolder, "package.json"));
        File.Copy(Path.Combine(bridgeFolder, "main.js"), Path.Combine(electronApplicationFolder, "main.js"));
        File.Copy(Path.Combine(bridgeFolder, "preload.js"), Path.Combine(electronApplicationFolder, "preload.js"));

        await ZipFile.CreateFromDirectoryAsync(electronFolder, outputFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(buildFolder))
        {
            Directory.Delete(buildFolder, true);
        }
    }
}