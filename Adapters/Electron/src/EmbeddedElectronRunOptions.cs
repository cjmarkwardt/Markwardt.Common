namespace Markwardt;

public class EmbeddedElectronRunOptionsTag : ConstructorTag<EmbeddedElectronRunOptions>;

public record EmbeddedElectronRunOptions
{
    public Action<IServiceConfiguration>? SetupServices { get; init; }
    public IServiceSource? ServiceSource { get; init; }
    public EmbeddedElectronOptions EmbedOptions { get; init; } = new();
    public FrontendWindowOpenOptions OpenOptions { get; init; } = new();
    public bool OutputToConsole { get; init; } = true;
}