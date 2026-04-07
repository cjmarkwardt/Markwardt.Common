namespace Markwardt;

public record FrontendWindowOpenOptions
{
    public int Width { get; init; } = 1280;
    public int Height { get; init; } = 720;
    public bool IsMaximized { get; init; } = false;
    public bool IsFullscreen { get; init; } = false;
    public bool EnableFullscreenShortcut { get; init; } = true;
    public bool ShowDevTools { get; init; } = false;
    public bool EnableDevToolsShortcut { get; init; } = false;
}