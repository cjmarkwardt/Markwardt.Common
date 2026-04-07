namespace Markwardt;

public record TextGeneration(TextGenerationKind Kind, string Value)
{
    public static IEnumerable<string> Serialize(IEnumerable<object> values)
    {
        foreach (object value in values)
        {
            if (value is string text)
            {
                yield return text;
            }
            else
            {
                yield return JsonSerializer.Serialize(value, value.GetType());
            }
        }
    }

    public static object Deserialize(Type? outputType, string value)
    {
        if (outputType is null || outputType == typeof(string))
        {
            return value;
        }
        else
        {
            return JsonSerializer.Deserialize(value, outputType) ?? throw new InvalidOperationException($"Failed to deserialize {value}");
        }
    }

    public static IEnumerable<TextGeneration> Command(params IEnumerable<object> values)
        => Serialize(values).Select(x => new TextGeneration(TextGenerationKind.Command, x));

    public static IEnumerable<TextGeneration> Request(params IEnumerable<object> values)
        => Serialize(values).Select(x => new TextGeneration(TextGenerationKind.Request, x));

    public static IEnumerable<TextGeneration> Response(params IEnumerable<object> values)
        => Serialize(values).Select(x => new TextGeneration(TextGenerationKind.Response, x));
}