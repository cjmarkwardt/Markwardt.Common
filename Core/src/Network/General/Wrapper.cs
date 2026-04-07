namespace Markwardt;

[DataContract]
public record Wrapper<T>([property: DataMember(Order = 1)] T Value)
{
    private Wrapper()
        : this(default(T)!) { }
}