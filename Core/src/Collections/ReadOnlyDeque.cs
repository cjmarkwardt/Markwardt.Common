namespace Markwardt;

public interface IReadOnlyDeque<T> : IReadOnlyCollection<T>
{
    IEnumerable<T> PeekDequeue(int count);
    IEnumerable<T> PeekPop(int count);
}

public static class ReadOnlyDequeExtensions
{
    public static Maybe<T> MaybePeekDequeue<T>(this IReadOnlyDeque<T> deque)
        => deque.PeekDequeue(1).Select(x => x.Maybe()).FirstOrDefault();

    public static bool TryPeekDequeue<T>(this IReadOnlyDeque<T> deque, [MaybeNullWhen(false)] out T item)
        => deque.MaybePeekDequeue().TryGetValue(out item);

    public static T PeekDequeue<T>(this IReadOnlyDeque<T> deque)
        => deque.MaybePeekDequeue().Value;

    public static Maybe<T> MaybePeekPop<T>(this IReadOnlyDeque<T> deque)
        => deque.PeekPop(1).Select(x => x.Maybe()).FirstOrDefault();

    public static bool TryPeekPop<T>(this IReadOnlyDeque<T> deque, [MaybeNullWhen(false)] out T item)
        => deque.MaybePeekPop().TryGetValue(out item);

    public static T PeekPop<T>(this IReadOnlyDeque<T> deque)
        => deque.MaybePeekPop().Value;
}