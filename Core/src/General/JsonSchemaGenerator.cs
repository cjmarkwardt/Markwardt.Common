namespace Markwardt;

public interface IJsonSchemaGenerator
{
    string Generate(Type type);
}

public class JsonSchemaGenerator : IJsonSchemaGenerator
{
    public string Generate(Type type)
        => JsonSerializerOptions.Default.GetJsonSchemaAsNode(type, new JsonSchemaExporterOptions()
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) =>
            {
                ICustomAttributeProvider? attributeProvider = context.PropertyInfo is not null
                    ? context.PropertyInfo.AttributeProvider
                    : context.TypeInfo.Type;

                DescriptionAttribute? attribute = attributeProvider?
                    .GetCustomAttributes(inherit: true)
                    .Select(x => x as DescriptionAttribute)
                    .FirstOrDefault(x => x is not null);

                if (attribute is not null)
                {
                    if (schema is not JsonObject target)
                    {
                        JsonValueKind valueKind = schema.GetValueKind();
                        schema = target = [];
                        if (valueKind is JsonValueKind.False)
                        {
                            target.Add("not", true);
                        }
                    }

                    target.Insert(0, "description", attribute.Description);
                }

                return schema;
            }
        }).ToString();
}