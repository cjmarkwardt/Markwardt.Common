namespace Markwardt;

public interface IEmbeddedElectronBuilder
{
    ValueTask<IFrontendWindow<T>> Build<T>(EmbeddedElectronOptions? options = null);
}

public class EmbeddedElectronBuilder : IEmbeddedElectronBuilder
{
    public async ValueTask<IFrontendWindow<T>> Build<T>(EmbeddedElectronOptions? options = null)
    {
        options ??= new EmbeddedElectronOptions();
        Assembly? assembly = options.Assembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = assembly.GetName();
        string name = assemblyName.Name ?? "EmbeddedElectronApp";
        string version = assemblyName.Version?.ToString() ?? string.Empty;

        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name, "EmbeddedElectron");
        string versionPath = Path.Combine(folderPath, "version.txt");
        string frontendUrl = options.FrontendUrl ?? $"file://{Path.Combine(folderPath, "frontend")}";
        string electronDeployPath = options.ElectronDeployPath ?? Path.Combine(folderPath, "electron");

        Directory.CreateDirectory(folderPath);

        string? deployedVersion = null;
        if (!options.OverwriteDeployment)
        {
            try { deployedVersion = await File.ReadAllTextAsync(versionPath); }
            catch (FileNotFoundException) { }
        }

        if (options.OverwriteDeployment || version != deployedVersion)
        {
            try { Directory.Delete(electronDeployPath, true); }
            catch (DirectoryNotFoundException) { }

            await assembly.GetManifestResourceStream("electron").NotNull("Electron app is not embedded in the assembly").Using(async resource => await Task.Run(() => ZipFile.ExtractToDirectory(resource, electronDeployPath)));

            if (frontendUrl.StartsWith("file://"))
            {
                string frontendDeployPath = frontendUrl["file://".Length..];

                try { Directory.Delete(frontendDeployPath, true); }
                catch (DirectoryNotFoundException) { }

                await assembly.GetManifestResourceStream("frontend").NotNull("Electron frontend is not embedded in the assembly").Using(async resource => await Task.Run(() => ZipFile.ExtractToDirectory(resource, frontendDeployPath)));
            }

            await File.WriteAllTextAsync(versionPath, version);
        }

        string executablePath;
        if (OperatingSystem.IsWindows())
        {
            executablePath = Path.Combine(electronDeployPath, "electron.exe");
        }
        else if (OperatingSystem.IsLinux())
        {
            executablePath = Path.Combine(electronDeployPath, "electron");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        return new ElectronWindow<T>(executablePath, $"{frontendUrl}/index.html");
    }
}