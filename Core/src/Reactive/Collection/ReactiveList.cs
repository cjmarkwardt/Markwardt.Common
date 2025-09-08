namespace Markwardt;

public interface IReactiveList<T> : ISourceList<T>, IReactiveAttachable<IEnumerable<ItemChange<T>>>;

public class ReactiveList<T> : IReactiveList<T>
{
    
}