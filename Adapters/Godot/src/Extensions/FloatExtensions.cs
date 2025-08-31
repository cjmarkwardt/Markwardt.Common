namespace Markwardt;

public static class FloatExtensions
{
    public static float Damp(this float value, float target, float smoothing, float deltaTime, float cutoff = 0)
    {
        if (Mathf.Abs(value - target) < cutoff)
        {
            return target;
        }
        else
        {
            return Mathf.Lerp(value, target, 1 - Mathf.Pow(smoothing, deltaTime));
        }
    }
}