namespace Markwardt;

public interface ISourceAttachable<T>
{
    void Attach(IObservable<T> source);
    void Detach();
}