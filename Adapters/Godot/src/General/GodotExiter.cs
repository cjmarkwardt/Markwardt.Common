namespace Markwardt;

public class GodotExiter(Window window) : IExiter
{
    private bool isExiting;

    public void Exit()
    {
        if (!isExiting)
        {
            isExiting = true;
            window.PropagateNotification((int)Node.NotificationWMCloseRequest);
        }
    }
}