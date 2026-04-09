namespace Markwardt;

public interface IFrontendWindow : ISender, IDisposable
{
    FrontendWindowState State { get; }

    IObservable<Exception?> Closed { get; }
    IObservable<string> Output { get; }
    IObservable<Packet> Received { get; }

    ValueTask Open(FrontendWindowOpenOptions? options = null);
}

public interface IFrontendWindow<T> : IFrontendWindow, ISender<T>;

public static class FrontendWindowExtensions
{
    public static IDisposable OutputToConsole(this IFrontendWindow window, string? prefix = null)
        => window.Output.Subscribe(x => Console.WriteLine($"{prefix ?? "[Frontend] "}{x}"));

    public static async ValueTask Run(this IFrontendWindow window, FrontendRunOptions? options = null)
    {
        options ??= new FrontendRunOptions();

        IDisposable? outputSubscription = null;
        IDisposable? setupSubscription = null;

        try
        {
            if (options.OutputToConsole)
            {
                outputSubscription = window.OutputToConsole();
            }

            await window.Open(options.OpenOptions);

            if (options.Setup is not null)
            {
                setupSubscription = options.Setup.Invoke(window);
            }

            await window.Closed;
        }
        finally
        {
            outputSubscription?.Dispose();
            setupSubscription?.Dispose();
        }
    }
}