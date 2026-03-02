namespace Markwardt;

[ServiceType<ISourceValue<string>>]
public class WorldSchemaTag : ServiceTag
{
    protected override object Resolve(IServiceProvider services)
        => new SourceValue<string>();
}