namespace Markwardt;

public interface IServicePackage
{
    void Configure(IServiceConfiguration configuration);
}

[AttributeUsage(AttributeTargets.Class)]
public class ConfigureServicesAttribute(Type servicePackage) : Attribute
{
    public Type ServicePackage => servicePackage;
}

[AttributeUsage(AttributeTargets.Class)]
public class ConfigureServicesAttribute<TServicePackage> : ConfigureServicesAttribute
    where TServicePackage : IServicePackage
{
    public ConfigureServicesAttribute()
        : base(typeof(TServicePackage)) { }
}

[AttributeUsage(AttributeTargets.Class)]
public class InitializeAttribute(Type initializer) : Attribute
{
    public Type Initializer => initializer;
}

[AttributeUsage(AttributeTargets.Class)]
public class InitializeAttribute<TInitializer> : InitializeAttribute
    where TInitializer : IAsyncInitializer
{
    public InitializeAttribute()
        : base(typeof(TInitializer)) { }
}

public class SingleThreadRunner
{
    public static void Run(IEnumerable<Type> types, Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        => SingleThreadContext.Run(async () => await ServiceProvider.Run(types, setup, source));

    public static void Run<TStarter>(Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        where TStarter : IStarter
        => SingleThreadContext.Run(async () => await ServiceProvider.Run<TStarter>(setup, source));
}

public class TerminalRunner
{
    public static void Run(IEnumerable<Type> types, Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        => AsyncContext.Run(async () => await ServiceProvider.Run(types, setup, source));

    public static void Run<TStarter>(Action<IServiceConfiguration>? setup = null, IServiceSource? source = null)
        where TStarter : IStarter
        => AsyncContext.Run(async () => await ServiceProvider.Run<TStarter>(setup, source));
}