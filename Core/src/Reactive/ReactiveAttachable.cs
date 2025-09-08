namespace Markwardt;

public interface IReactiveAttachable<T>
{
    void Attach(IObservable<T> source);
    void Detach();
}