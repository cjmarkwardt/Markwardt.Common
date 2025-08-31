namespace Markwardt;

public class TerminalRunner
{
    public static void Run<TStarter>(Action<IServiceConfiguration>? configure = null)
        where TStarter : IStarter
        => AsyncContext.Run(async () =>
        {
            await using ServiceContainer services = new();
            await services.Start<TStarter>(configure);
        });
}