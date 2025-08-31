namespace Markwardt;

public interface IScreenLayerOrganizer
{
    int GetOrder(string layer);
    void SetOrder(string layer, int order);
}

public class ScreenLayerOrganizer : IScreenLayerOrganizer
{
    private readonly Dictionary<string, int> layers = [];

    public int GetOrder(string layer)
        => layers.GetValueOrDefault(layer);

    public void SetOrder(string layer, int order)
        => layers[layer] = order;
}