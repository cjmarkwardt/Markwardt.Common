namespace Markwardt;

public interface ISpriteEditor
{
    IEnumerable<object> Elements { get; }

    void SetElement(object element, SpriteComposite composite);
    void ClearElement(object element);
    void SetFlip(bool value);
    void SetColor(object color, Color value);
    void SetAnchor(object? anchor, Vector2 value);
    void PlayAnimation(object? animation, bool replay = false, object? animator = default);
    void ResetAnimator(object? animator);
}