namespace Markwardt;

public static class GodotServiceConfigurationExtensions
{
    public static void ConfigureProjectSetting<TTag>(this IServiceConfiguration configuration, string name, Func<Variant, object> cast)
        where TTag : notnull, IServiceTag
        => configuration.Configure<TTag>(new Service(_ => cast(ProjectSettings.GetSetting(name))));
}