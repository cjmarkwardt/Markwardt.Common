namespace Markwardt;

public record struct DictionaryEntry<TKey, TValue>(TKey Key, Func<TValue> GetValue);