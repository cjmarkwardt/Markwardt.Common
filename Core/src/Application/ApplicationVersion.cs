namespace Markwardt;

[ServiceType<string>]
public class ApplicationVersionTag : ServiceTag
{
    protected override object GetService(IServiceProvider services)
        => (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.NotNull().ToString();
}