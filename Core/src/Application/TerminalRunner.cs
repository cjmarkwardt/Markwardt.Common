namespace Markwardt;

public class TerminalRunner
{
    public static void Run<TStarter>(IServiceSource? source = null, Action<IServiceContainer>? setup = null)
        where TStarter : IStarter
        => AsyncContext.Run(async () =>
        {
            using ServiceContainer services = new(source);
            
            if (setup is not null)
            {
                setup(services);
            }
            
            await services.Start<TStarter>();
        });
}