namespace Markwardt;

[ServiceType<string>]
public class ApplicationVersionTag : SimpleTag
{
    protected override object Get()
        => (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.NotNull().ToString();
}