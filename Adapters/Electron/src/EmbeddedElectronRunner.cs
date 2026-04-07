namespace Markwardt;

public static class EmbeddedElectronRunner
{
    public static void Run<TMessage>(IEnumerable<Type> starters, EmbeddedElectronRunOptions? options = null)
    {
        options ??= new EmbeddedElectronRunOptions();

        SingleThreadRunner.Run(starters.Prepend(typeof(EmbeddedElectronStarter<TMessage>)), source: options.ServiceSource, setup: services =>
        {
            services.Configure<EmbeddedElectronRunOptionsTag>(new InstanceService(options));
            options.SetupServices?.Invoke(services);
        });
    }

    public static void Run<TMessage>(EmbeddedElectronRunOptions? options = null)
        => Run<TMessage>([], options);

    public static void Run<TStarter, TMessage>(EmbeddedElectronRunOptions? options = null)
        where TStarter : IStarter
        where TMessage : notnull
        => Run<TMessage>([typeof(TStarter)], options);
}