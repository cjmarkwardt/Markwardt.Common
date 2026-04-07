namespace Markwardt;

[FactoryConstructor<OpenAiGenerator>]
public delegate IGenerator OpenAiGeneratorFactory(string apiKey);

public class OpenAiGenerator(string apiKey)
    : CompositeGenerator
    (
        new OpenAiTextGenerator(apiKey, ["gpt-5-nano", "gpt-5-mini", "gpt-5.1"]),
        new OpenAiImageGenerator(apiKey, ["gpt-image-1-mini", "gpt-image-1"]),
        new OpenAiSpeechGenerator(apiKey, ["tts-1", "tts-1-hd"])
    );