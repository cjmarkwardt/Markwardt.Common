namespace Markwardt;

public class SingleThreadRunner
{
    public static void Run(IEnumerable<Type> types, Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        => SingleThreadContext.Run(async () => await ServiceProvider.Run(types, setup, source));

    public static void Run<TStarter>(Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        where TStarter : IStarter
        => SingleThreadContext.Run(async () => await ServiceProvider.Run<TStarter>(setup, source));
}