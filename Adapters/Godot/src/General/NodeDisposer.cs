namespace Markwardt;

public class NodeDisposer : CompositeDisposable
{
    public void ReadNotification(int notification)
    {
        if (notification is (int)GodotObject.NotificationPredelete)
        {
            Dispose();
        }
    }
}