namespace Markwardt;

public interface ICompositeDisposable : IDisposable
{
    ISet<object> DisposalTargets { get; }
}

public static class CompositeDisposableExtensions
{
    public static T DisposeWith<T>(this T target, ICompositeDisposable disposables)
    {
        if (target is not null)
        {
            disposables.DisposalTargets.Add(target);
        }

        return target;
    }
}

public class CompositeDisposable : ICompositeDisposable
{
    public ISet<object> DisposalTargets { get; } = new HashSet<object>();

    public void Dispose()
    {
        foreach (object target in DisposalTargets)
        {
            target.TryDispose();
        }

        DisposalTargets.Clear();
    }
}