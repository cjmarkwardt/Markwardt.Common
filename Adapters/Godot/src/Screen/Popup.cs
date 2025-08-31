using System.Diagnostics;

namespace Markwardt;

public interface IPopup : IDisposable
{
    IPopup WithTitle(string title);
    IPopup WithError(string error);
    IPopup WithInput(ITextValidator? validator, Action<string> action);
    IPopup WithButton(string name, Action action);
    IPopup WithItem(Action<IPopupItem> configure);
    IPopup WithClose(Action close);

    void Close();
}

public static class PopupExtensions
{
    public static IPopup WithItems<T>(this IPopup popup, IEnumerable<T> values, Action<T, IPopupItem> configure)
    {
        foreach (T value in values)
        {
            popup.WithItem(item => configure(value, item));
        }

        return popup;
    }

    public static IPopup WithItems<T>(this IPopup popup, IAsyncEnumerable<T> values, Action<T, IPopupItem> configure)
    {
        async void Populate()
        {
            await foreach (T value in values)
            {
                popup.WithItem(item => configure(value, item));
            }
        }

        Populate();
        return popup;
    }

    public static IPopup WithInput(this IPopup popup, Action<string> action)
        => popup.WithInput(null, action);

    public static async ValueTask<bool> QueryYesNo(this IPopup popup)
    {
        TaskCompletionSource<bool> completion = new();
        popup.WithButton("Yes", () => completion.SetResult(true)).WithButton("No", () => completion.SetResult(false));
        return await completion.Task;
    }

    public static async ValueTask<string> QueryText(this IPopup popup, ITextValidator? validator = null)
    {
        TaskCompletionSource<string> completion = new();
        popup.WithInput(validator, completion.SetResult);
        return await completion.Task;
    }

    public static async ValueTask Confirm(this IPopup popup)
    {
        TaskCompletionSource completion = new();
        popup.WithButton("Ok", completion.SetResult);
        await completion.Task;
    }

    public static void Notify(this IPopup popup)
        => popup.WithButton("Ok", () => { });
}

public partial class Popup : MarginContainer, IPopup
{
    public Popup()
    {
        this.WithLayoutPreset(LayoutPreset.FullRect);
        this.WithColorBackground(Colors.Black.WithAlpha(0.5f));

        PanelContainer panel = new PanelContainer().WithCenterParent(this).WithColorBackground(Colors.Black);
        layout = new VBoxContainer().WithMarginParent(panel, 50, 20).WithAlignment(BoxContainer.AlignmentMode.Center);

        title = new Label().WithMarginParent(layout, 0, 0, 0, 20);
        title.Visible = false;

        error = new Label().WithParent(layout).WithFontColor(Colors.Red);
        error.Visible = false;

        buttons = new HBoxContainer().WithParent(layout).WithAlignment(BoxContainer.AlignmentMode.Center);
        items = new VBoxContainer().WithParent(layout).WithAlignment(BoxContainer.AlignmentMode.Center);
    }

    private readonly VBoxContainer layout;
    private readonly Label title;
    private readonly Label error;
    private readonly HBoxContainer buttons;
    private readonly VBoxContainer items;
    private readonly List<Action> closes = [];

    public required PopupItem.Factory ItemFactory { get; init; }

    public IPopup WithTitle(string title)
        => this.Do(_ =>
        {
            this.title.Visible = true;
            this.title.Text = title;
        });

    public IPopup WithError(string error)
        => this.Do(_ =>
        {
            this.error.Visible = true;
            this.error.Text = error;
        });

    public IPopup WithInput(ITextValidator? validator, Action<string> action)
        => this.Do(_ => new LineEdit().WithParent(layout).OnTextSubmitted(input =>
        {
            Failable tryValidate = validator is not null ? validator.Validate(input) : Failable.Success();
            if (tryValidate.Exception is not null)
            {
                WithError(tryValidate.Exception.Message);
            }
            else
            {
                Close();
                action(input);
            }
        }));

    public IPopup WithButton(string name, Action action)
        => this.Do(_ => new Button().WithParent(buttons).WithText(name).OnPressed(() =>
        {
            Close();
            action();
        }));

    public IPopup WithItem(Action<IPopupItem> configure)
        => this.Do(_ => configure(ItemFactory(this).WithParent(items)));

    public IPopup WithClose(Action close)
    {
        closes.Add(close);
        return this;
    }

    public void Close()
    {
        QueueFree();

        foreach (Action close in closes)
        {
            close();
        }
    }
}