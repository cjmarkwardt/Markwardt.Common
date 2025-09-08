namespace Markwardt;

public interface ISubject : ISubject<bool>, IObservable;

public sealed class GeneralSubject : GeneralObserver, ISubject, IDisposable
{
    private readonly Subject<bool> subject = new();

    public void Dispose()
        => subject.Dispose();

    public override void OnCompleted()
        => subject.OnCompleted();

    public override void OnError(Exception error)
        => subject.OnError(error);

    public override void OnNext()
        => subject.OnNext(default);

    public IDisposable Subscribe(IObserver<bool> observer)
        => subject.Subscribe(observer);
}
