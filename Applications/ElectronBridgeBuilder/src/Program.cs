List<ElectronVersion> versions = args[0].Split(',').SelectMany(x => args[1].Split(',').Select(p => new ElectronVersion(x, p))).ToList();

string bridgeFolder = Path.Combine(Environment.CurrentDirectory, "bridge");
string buildFolder = Path.Combine(Environment.CurrentDirectory, "build");
string outputFolder = Path.Combine(Environment.CurrentDirectory, "output");

using Builder builder = new(bridgeFolder, buildFolder, outputFolder);

async Task Build(ElectronVersion version)
{
    Console.WriteLine($"Building {version.Id}...");
    await builder.Build(version);
    Console.WriteLine($"Built {version.Id}");
}

await Task.WhenAll(versions.Select(Build));