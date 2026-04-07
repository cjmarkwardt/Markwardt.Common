namespace Markwardt;

public class BusyFlag : IDisposable
{
    private bool isBusy;

    public BusyFlag Set()
    {
        if (isBusy)
        {
            throw new BusyException();
        }

        isBusy = true;
        return this;
    }

    public void Dispose()
        => isBusy = false;
}