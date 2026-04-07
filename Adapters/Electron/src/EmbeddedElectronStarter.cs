namespace Markwardt;

public class EmbeddedElectronStarter<T> : IStarter
{
    [Inject<EmbeddedElectronRunOptionsTag>]
    public required EmbeddedElectronRunOptions Options { get; init; }

    public required IEmbeddedElectronBuilder Builder { get; init; }
    
    [Inject<FrontendSenderTag>]
    public required IConfiguredMessageSender FrontendSender { get; init; }

    [Inject<MessagesTag>]
    public required IEvent<Message> Messages { get; init; }

    [Inject<MessagesInitializerTag>]
    public required IInitializer MessagesInitializer { get; init; }

    [Inject<ExitedTag>]
    public required ISignal<Exception?> Exited { get; init; }

    public async ValueTask Start(CancellationToken cancellation = default)
    {
        CompositeDisposable disposables = new();

        IFrontendWindow<T> window = (await Builder.Build<T>(Options.EmbedOptions)).DisposeWith(disposables);

        FrontendSender.Configure(window);
        MessagesInitializer.Initialize();

        if (Options.OutputToConsole)
        {
            window.OutputToConsole().DisposeWith(disposables);
        }

        window.Received.Subscribe(Messages.Invoke).DisposeWith(disposables);

        window.Closed.Subscribe(exception =>
        {
            disposables.Dispose();
            Exited.Set(exception);
        }).DisposeWith(disposables);

        await window.Open(Options.OpenOptions);
    }
}