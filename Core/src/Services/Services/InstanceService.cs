namespace Markwardt;

public class InstanceService(object? instance, bool dispose = true) : BaseDisposable, IService
{
    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => instance;

    protected override void OnDispose()
    {
        base.OnDispose();

        if (dispose)
        {
            instance.TryDispose();
        }
    }

    public override string ToString()
        => $"{base.ToString()} (Instance: {instance}, Dispose: {dispose})";
}