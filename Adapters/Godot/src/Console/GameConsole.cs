namespace Markwardt;

public interface IGameConsole
{
    bool IsVisible { get; set; }

    void Print(string message);
    void Configure(string command, Action<IReadOnlyList<string>> action);
}

public static class GameConsoleExtensions
{
    public static void Configure(this IGameConsole console, Action<IReadOnlyList<string>> action)
        => console.Configure(action.Method.Name, action);
}

public partial class GameConsole : ColorRect, IGameConsole
{
    public static string Layer => $"Console:{Guid.NewGuid():N}";

    public GameConsole(IScreenOpener opener)
    {
        Visible = false;

        this.WithAnchors(0, 1, 0, 0.5f);
        this.WithColor(Colors.DarkSlateGray);

        VBoxContainer layout = new VBoxContainer().WithParent(this).WithLayoutPreset(LayoutPreset.FullRect);
        scroll = new ScrollContainer().WithParent(layout).WithVerticalSizeFlag(SizeFlags.ExpandFill);
        outputs = new VBoxContainer().WithParent(scroll);
        input = new LineEdit().WithParent(layout).WithVerticalSizeFlag(SizeFlags.Fill).OnTextSubmitted(OnSubmit);

        opener.Open(this, Layer);
    }

    private readonly Dictionary<string, Action<IReadOnlyList<string>>> commands = [];
    private readonly ScrollContainer scroll;
    private readonly VBoxContainer outputs;
    private readonly LineEdit input;

    public new bool IsVisible { get => Visible; set => Visible = value; }

    [Inject<ConsoleInputTag>]
    public required IGameInput Trigger { get; init; }

    public void Print(string message)
    {
        new Label().WithParent(outputs).WithText(message);
    }

    public void Configure(string command, Action<IReadOnlyList<string>> action)
        => commands[command.ToLower()] = action;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Trigger.IsJustPressed)
        {
            IsVisible = !IsVisible;
        }
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        input.Text = string.Empty;
        Print($"---> {text}");

        string[] arguments = text.Split(' ').Select(x => x.Replace("\\ ", " ").Replace("\\\\", "\\")).ToArray();
        if (arguments.Length > 0)
        {
            string command = arguments[0];
            if (commands.TryGetValue(command.ToLower(), out Action<IReadOnlyList<string>>? action))
            {
                action(arguments[1..]);
            }
            else
            {
                Print($"Unknown command {command}");
            }
        }
    }
}