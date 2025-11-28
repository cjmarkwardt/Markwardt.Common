namespace Markwardt;

[Inject<IScreenController>]
public interface IScreenOpener
{
    void Open(Control control, string layer);
}