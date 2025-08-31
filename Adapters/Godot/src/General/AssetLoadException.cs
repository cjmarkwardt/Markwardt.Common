namespace Markwardt;

public class AssetLoadException(string path, string reason) : Exception($"Failed to load {path} ({reason})");