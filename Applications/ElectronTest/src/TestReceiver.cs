namespace Markwardt.ElectronTest;

public class TestReceiver : Receiver<Message>
{
    protected override void Receive(Packet<Message> packet)
    {
        if (packet.Content.TestRequest is TestRequest request)
        {
            Console.WriteLine($"Got request from {request.Name}: {request.Value}");
            packet.Respond(new Message() { TestResponse = new($"Hello from Backend, {request.Name}!") });
        }
    }
}