namespace Markwardt;

public partial class ServiceInitializer<TStarter> : Node
    where TStarter : notnull, IStarter
{
    private readonly ServiceContainer services = new();

    private SceneTree tree = null!;
    private IEvent<InputEvent> inputEvent = null!;

    public override async void _Ready()
    {
        Configure(services);
        Setup(services);
        await services.Start<TStarter>();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            services.TryDispose();
            tree.Quit();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        inputEvent?.Invoke(@event);
    }

    protected virtual void Configure(IServiceConfiguration services)
    {
        services.Configure<SceneTree>(Service.Instance(GetTree()));
        services.Configure<Window>(Service.Instance(GetTree().Root));
        services.Configure<RootNodeTag>(Service.Instance(this));
        services.Configure<ILogger, GodotLogger>();
        services.Configure<IExiter, GodotExiter>();
        services.ConfigureProjectSetting<ApplicationNameTag>("application/config/name", x => x.AsString());
        services.ConfigureProjectSetting<ApplicationVersionTag>("application/config/version", x => x.AsString());
    }

    protected virtual void Setup(IServiceContainer services)
    {
        tree = GetTree();
        tree.AutoAcceptQuit = false;

        inputEvent = services.GetRequiredService<GameInputEventTag, IEvent<InputEvent>>();
    }
}