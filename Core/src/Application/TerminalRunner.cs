namespace Markwardt;

public class TerminalRunner
{
    public static void Run<TStarter>(Action<IServiceContainer>? setup = null)
        where TStarter : IStarter
        => AsyncContext.Run(async () =>
        {
            await using ServiceContainer services = new();
            
            if (setup is not null)
            {
                setup(services);
            }
            
            await services.Start<TStarter>();
        });
}