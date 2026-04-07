namespace Markwardt;

public record ValueWrapper<T>
    where T : struct
{
    public T Value { get; set; }
}