namespace Markwardt;

public interface IScreenController : IScreenOpener, IScreenClearer;

public class ScreenController([Inject<RootNodeTag>] Node sceneRoot, IScreenLayerOrganizer organizer) : IScreenController
{
    private readonly SortedDictionary<string, Control> layers = new(new LayerComparer(organizer));

    private Control root = new Control().WithName("Screen").WithParent(sceneRoot).WithAnchorsPreset(Control.LayoutPreset.FullRect);

    public void Open(Control control, string layer)
        => control.WithParent(GetLayer(layer));

    public void Clear()
    {
        layers.Clear();
        root.QueueFree();
        root = new Control().WithName("Screen").WithParent(sceneRoot);
    }

    private Control GetLayer(string layerId)
    {
        if (!layers.TryGetValue(layerId, out Control? layer))
        {
            layer = new Control().WithName($"Layer {layerId}").WithParent(root).WithAnchorsPreset(Control.LayoutPreset.FullRect);
            layers.Add(layerId, layer);
            Resort();
        }

        return layer;
    }

    private async void Resort()
    {
        await Task.Delay(100);
        layers.ForEach(x => x.Value.MoveToFront());
    }

    private sealed class LayerComparer(IScreenLayerOrganizer organizer) : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            ArgumentNullException.ThrowIfNull(x);
            ArgumentNullException.ThrowIfNull(y);

            int orderComparison = GetOrder(x).CompareTo(GetOrder(y));
            if (orderComparison == 0)
            {
                return x.CompareTo(y);
            }

            return orderComparison;
        }

        private int GetOrder(string? layer)
            => layer is null ? 0 : organizer.GetOrder(layer);
    }
}