namespace Markwardt;

public static class ObservableExtensions
{
    public static void Subscribe<T>(this IObservable<T> source, IEvent<T> target)
        => source.Subscribe(target.Invoke);

    public static IObservable<T> AsObservable<T>(this T value)
        => Observable.Never<T>().StartWith(value);

    public static IObservable WithoutResult<T>(this IObservable<T> source)
        => new GeneralObservable(source.Select(_ => false));

    public static IObservable<TChained> Chain<T, TChained>(this IObservable<T> source, Func<T, IObservable<TChained>> chain)
        => source.Select(x => chain(x)).Merge();

    public static IObservable TriggerOnce(this IObservable source)
        => new TriggerOnceObservable(source);

    private class TriggerOnceObservable(IObservable source) : IObservable
    {
        public IDisposable Subscribe(IObserver<bool> observer)
        {
            IDisposable subscription = source.Subscribe(observer);
            observer.OnNext(default);
            return subscription;
        }
    }
}