namespace Markwardt;

public record SpriteAnimation(SpriteAtlas Atlas, float FrameRate, IReadOnlyList<SpriteFrame> Frames)
{
    public static implicit operator SpriteAnimation(SpriteAtlas atlas)
        => new(atlas, 1, SpriteFrame.FromImages(0, 1));

    public static float GetElapsedTime(float start)
        => Time.Singleton.GetElapsed() - start;

    public SpriteAnimation(SpriteAtlas atlas)
        : this(atlas, 1, [new()]) { }

    public Vector2 Offset { get; init; }

    public float GetElapsedFrames(float start)
        => GetElapsedTime(start) * FrameRate;

    public int GetCurrentFrameIndex(float start)
        => Mathf.FloorToInt(GetElapsedFrames(start) % Frames.Count);

    public SpriteFrame GetCurrentFrame(float start, bool offset = true)
    {
        SpriteFrame frame = Frames[GetCurrentFrameIndex(start)];
        if (offset)
        {
            frame = frame with { Offset = frame.Offset + Offset };
        }

        return frame;
    }
}