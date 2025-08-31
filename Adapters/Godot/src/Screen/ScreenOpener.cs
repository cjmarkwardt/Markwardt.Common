namespace Markwardt;

[RouteService<IScreenController>]
public interface IScreenOpener
{
    void Open(Control control, string layer);
}