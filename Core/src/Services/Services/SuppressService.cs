namespace Markwardt;

public class SuppressService : BaseDisposable, IService
{
    public static SuppressService Instance { get; } = new();

    private SuppressService() { }

    public object? Resolve(IServiceProvider services, IEnumerable<ServiceOverride> overrides)
        => Signal.Instance;

    public override string ToString()
        => $"SuppressService";

    public class Signal
    {
        public static object Instance { get; } = new();

        private Signal() { }
    }
}