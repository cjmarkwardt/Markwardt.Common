namespace Markwardt;

public partial class ServiceInitializer<TStarter> : Node
    where TStarter : notnull, IStarter
{
    private readonly ServiceContainer services = new();

    public override async void _Ready()
        => await services.Start<TStarter>(Configure);

    protected virtual void Configure(IServiceConfiguration configuration)
    {
        GetTree().AutoAcceptQuit = false;

        configuration.ConfigureInstance<Window>(GetTree().Root);
        configuration.ConfigureInstance<SceneTree>(GetTree());
        configuration.ConfigureImplementation<ILogger, GodotLogger>();
        configuration.ConfigureImplementation<IExiter, GodotExiter>();
        configuration.ConfigureProjectSetting<ApplicationNameTag>("application/config/name", x => x.AsString());
        configuration.ConfigureProjectSetting<ApplicationVersionTag>("application/config/version", x => x.AsString());
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            services.TryDispose();
            GetTree().Quit();
        }
    }
}