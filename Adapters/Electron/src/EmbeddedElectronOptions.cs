namespace Markwardt;

public record EmbeddedElectronOptions
{
    public Assembly? Assembly { get; init; }
    public string? FrontendUrl { get; init; }
    public string? ElectronDeployPath { get; init; }
    public bool OverwriteDeployment { get; init; } = true;
}