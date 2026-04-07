namespace Markwardt;

public interface IConverter<T, TConverted>
{
    TConverted Convert(T value);
    T Revert(TConverted value);
}

public class Converter<T, TConverted>(Func<T, TConverted> convert, Func<TConverted, T> revert) : IConverter<T, TConverted>
{
    public TConverted Convert(T value) => convert(value);
    public T Revert(TConverted value) => revert(value);
}