namespace Markwardt;

[ServiceType<string>]
public class ApplicationNameTag : SimpleTag
{
    protected override object Get()
        => (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Name.NotNull();
}