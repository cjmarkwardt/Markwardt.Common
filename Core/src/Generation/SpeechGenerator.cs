namespace Markwardt;

public interface ISpeechGenerator
{
    ValueTask<BinaryData> GenerateSpeech(float quality, VoiceType type, float voice, string input, string script);
}