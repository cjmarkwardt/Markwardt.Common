namespace Markwardt;

public interface IPopupController
{
    IPopup Open();
}

public class PopupController : IPopupController
{
    public static string Layer => $"Popups:{Guid.NewGuid():N}";

    public required Factory<Popup> PopupFactory { get; init; }
    public required IScreenOpener Opener { get; init; }

    public IPopup Open()
    {
        Popup popup = PopupFactory().WithName(nameof(Popup));
        Opener.Open(popup, Layer);
        return popup;
    }
}