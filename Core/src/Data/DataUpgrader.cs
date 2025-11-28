namespace Markwardt;

public record DataUpgrade(string TargetSchema, AsyncFunction<object, object> Upgrade);

[ServiceType<IDictionary<string, DataUpgrade>>]
public class DataUpgradesTag : ConstructorTag<Dictionary<string, DataUpgrade>>;

public interface IDataUpgrader
{
    ValueTask<object> Upgrade(string schema, object value, CancellationToken cancellation = default);
}

public class DataUpgrader([Inject<DataUpgradesTag>] IReadOnlyDictionary<string, DataUpgrade> upgradePaths) : IDataUpgrader
{
    public async ValueTask<object> Upgrade(string schema, object value, CancellationToken cancellation = default)
    {
        while (upgradePaths.TryGetValue(schema, out DataUpgrade? upgradePath))
        {
            value = await upgradePath.Upgrade(value, cancellation);
            schema = upgradePath.TargetSchema;
        }

        return value;
    }
}