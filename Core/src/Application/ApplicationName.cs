namespace Markwardt;

[ServiceType<string>]
public class ApplicationNameTag : ServiceTag
{
    protected override object GetService(IServiceProvider services)
        => (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Name.NotNull();
}