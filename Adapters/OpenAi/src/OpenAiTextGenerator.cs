namespace Markwardt;

#pragma warning disable OPENAI001

public class OpenAiTextGenerator(string apiKey, IEnumerable<string> models) : ITextGenerator
{
    private readonly JsonSchemaGenerator schemaGenerator = new();
    private readonly ClientModelSet<ChatClient> model = new(models, model => new(model, apiKey));
    private readonly Dictionary<Type, ChatResponseFormat> formats = [];
    private readonly List<ChatReasoningEffortLevel> efforts = [ChatReasoningEffortLevel.Minimal, ChatReasoningEffortLevel.Low, ChatReasoningEffortLevel.Medium, ChatReasoningEffortLevel.High];

    public async ValueTask<string> GenerateText(float quality, float effort, Type? outputType, params IEnumerable<TextGeneration> inputs)
        => (await model.GetClient(quality).CompleteChatAsync(inputs.Select(CreateMessage), CreateOptions(effort, outputType))).Value.Content[0].Text;

    private ChatMessage CreateMessage(TextGeneration generation)
        => generation.Kind switch
        {
            TextGenerationKind.Command => ChatMessage.CreateSystemMessage(generation.Value),
            TextGenerationKind.Request => ChatMessage.CreateUserMessage(generation.Value),
            TextGenerationKind.Response => ChatMessage.CreateAssistantMessage(generation.Value),
            _ => throw new NotSupportedException(generation.Kind.ToString())
        };

    private ChatCompletionOptions CreateOptions(float effort, Type? outputType)
    {
        ChatCompletionOptions options = new() { ReasoningEffortLevel = efforts.GetPercentageItem(effort) };

        if (outputType is not null && outputType != typeof(string))
        {
            if (!formats.TryGetValue(outputType, out ChatResponseFormat? format))
            {
                format = ChatResponseFormat.CreateJsonSchemaFormat(GenericTypeNameAttribute.GetName(outputType), BinaryData.FromString(schemaGenerator.Generate(outputType)));
                formats[outputType] = format;
            }

            options.ResponseFormat = format;
        }

        return options;
    }
}