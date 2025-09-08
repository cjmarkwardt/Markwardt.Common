namespace Markwardt;

public interface ISourceCollection : IObservableCollection
{
    new IAccessor Accessor { get; }

    new interface IAccessor : IObservableCollection.IAccessor
    {
        void Add(object? item);
        void Remove(object? item);
        void Clear();
    }

    new interface IKeyedAccessor : IObservableCollection.IKeyedAccessor
    {
        void AddPair(object? key, object? item);
        void RemoveKey(object? key);
    }
}