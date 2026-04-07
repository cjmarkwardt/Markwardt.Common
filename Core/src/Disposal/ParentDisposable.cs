namespace Markwardt;

public interface IParentDisposable : IDisposable
{
    void AddChildDisposable(object? disposable);
    void RemoveChildDisposable(object? disposable);
}