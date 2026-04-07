namespace Markwardt;

public static class ObservableExtensions
{
    public static void Subscribe<T>(this IObservable<T> source, IEvent<T> target)
        => source.Subscribe(target.Invoke);

    public static IObservable<T> AsObservable<T>(this T value)
        => Observable.Never<T>().StartWith(value);

    public static IObservable<Unit> WithoutResult<T>(this IObservable<T> source)
        => source.Select(_ => Unit.Default);

    public static IObservable<TChained> Chain<T, TChained>(this IObservable<T> source, Func<T, IObservable<TChained>> chain)
        => source.Select(x => chain(x)).Merge();

    public static IObservable<TResult> SelectWhere<T, TResult>(this IObservable<T> source, Func<T, Maybe<TResult>> selector)
        => source.Select(selector).Where(x => x.HasValue).Select(x => x.Value);

    public static IObservable<Unit> TriggerOnce(this IObservable<Unit> source)
        => new TriggerOnceObservable(source);

    private class TriggerOnceObservable(IObservable<Unit> source) : IObservable<Unit>
    {
        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            IDisposable subscription = source.Subscribe(observer);
            observer.OnNext(Unit.Default);
            return subscription;
        }
    }
}