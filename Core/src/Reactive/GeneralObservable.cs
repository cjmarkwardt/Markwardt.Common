namespace Markwardt;

public interface IObservable : IObservable<bool> { }

public class GeneralObservable(IObservable<bool> source) : IObservable
{
    public IDisposable Subscribe(IObserver<bool> observer)
        => source.Subscribe(observer);
}