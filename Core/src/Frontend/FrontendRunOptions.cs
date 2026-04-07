namespace Markwardt;

public record FrontendRunOptions
{
    public FrontendWindowOpenOptions? OpenOptions { get; init; }
    public bool OutputToConsole { get; init; } = true;
    public Func<IFrontendWindow, IDisposable?>? Setup { get; init; }
}