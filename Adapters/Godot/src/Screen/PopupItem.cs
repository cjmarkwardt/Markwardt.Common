namespace Markwardt;

public interface IPopupItem : IDisposable
{
    IPopup Popup { get; }

    IPopupItem WithTitle(string title);
    IPopupItem WithAction(Action action);
    IPopupItem WithButton(string name, Action action);
}

public partial class PopupItem : MarginContainer, IPopupItem
{
    public delegate PopupItem Factory(IPopup popup);

    public PopupItem(IPopup popup)
    {
        Popup = popup;

        HBoxContainer layout = new HBoxContainer().WithParent(this).WithLayoutPreset(LayoutPreset.FullRect);
        button = new Button().WithParent(layout).WithHorizontalSizeFlag(SizeFlags.ExpandFill);
        buttons = new HBoxContainer().WithParent(layout).WithAlignment(BoxContainer.AlignmentMode.Center);
    }

    private readonly Button button;
    private readonly HBoxContainer buttons;

    public IPopup Popup { get; }

    public IPopupItem WithTitle(string title)
    {
        button.Visible = true;
        button.Text = title;
        return this;
    }

    public IPopupItem WithAction(Action action)
    {
        button.OnPressed(() =>
        {
            Popup.Close();
            action();
        });

        return this;
    }

    public IPopupItem WithButton(string name, Action action)
    {
        new Button().WithParent(buttons).WithText(name).OnPressed(() =>
        {
            Popup.Close();
            action();
        });

        return this;
    }
}