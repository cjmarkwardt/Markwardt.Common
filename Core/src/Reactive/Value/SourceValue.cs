namespace Markwardt;

public interface ISourceValue : IObservableValue
{
    new IAccessor Accessor { get; }

    new interface IAccessor : IObservableValue.IAccessor
    {
        void Set(object? value);
    }
}

public interface ISourceValue<T> : ISourceValue, IObservableValue<T>
{
    new T Value { get; set; }
}