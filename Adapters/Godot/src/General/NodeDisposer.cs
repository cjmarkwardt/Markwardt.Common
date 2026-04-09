namespace Markwardt;

public class NodeDisposer : BaseDisposable
{
    public void ReadNotification(int notification)
    {
        if (notification is (int)GodotObject.NotificationPredelete)
        {
            Dispose();
        }
    }
}