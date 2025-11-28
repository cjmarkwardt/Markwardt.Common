namespace Markwardt;

[ServiceType<ISourceValue<string>>]
public class WorldSchemaTag : SimpleTag
{
    protected override object Get()
        => new SourceValue<string>();
}