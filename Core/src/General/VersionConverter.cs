namespace Markwardt;

public interface IVersionConverter
{
    void Configure(string version, string targetVersion, Func<object, object> convert);
    object Convert(object value, string version, string targetVersion);
}

public class VersionConverter : IVersionConverter
{
    private readonly Dictionary<string, Conversion> conversions = [];

    public void Configure(string version, string convertedVersion, Func<object, object> convert)
        => conversions.Add(version, new(convertedVersion, convert));

    public object Convert(object value, string version, string targetVersion)
    {
        while (version != targetVersion)
        {
            Conversion conversion = conversions[version];
            value = conversion.Convert(value);
            version = conversion.Version;
        }

        return value;
    }

    public record Conversion(string Version, Func<object, object> Convert);
}