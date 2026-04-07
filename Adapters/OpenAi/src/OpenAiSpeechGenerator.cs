namespace Markwardt;

#pragma warning disable OPENAI001

public class OpenAiSpeechGenerator(string apiKey, IEnumerable<string> models) : ISpeechGenerator
{
    private readonly ClientModelSet<AudioClient> model = new(models, model => new(model, apiKey));

    private readonly Dictionary<VoiceType, List<GeneratedSpeechVoice>> voices = new()
    {
        {
            VoiceType.Masculine,
            new()
            {
                GeneratedSpeechVoice.Ash,
                GeneratedSpeechVoice.Ballad,
                GeneratedSpeechVoice.Echo,
                GeneratedSpeechVoice.Onyx,
                GeneratedSpeechVoice.Verse
            }
        },
        {
            VoiceType.Feminine,
            new()
            {
                GeneratedSpeechVoice.Fable,
                GeneratedSpeechVoice.Alloy,
                GeneratedSpeechVoice.Coral,
                GeneratedSpeechVoice.Nova,
                GeneratedSpeechVoice.Sage,
                GeneratedSpeechVoice.Shimmer
            }
        }
    };

    public async ValueTask<BinaryData> GenerateSpeech(float quality, VoiceType type, float voice, string input, string script)
        => await model.GetClient(quality).GenerateSpeechAsync(script, voices[type].GetPercentageItem(voice), new SpeechGenerationOptions()
        {
            ResponseFormat = GeneratedSpeechFormat.Wav,
            Instructions = input
        });
}