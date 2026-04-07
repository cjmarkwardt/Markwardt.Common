namespace Markwardt;

public class ClientModelSet<TClient>(IEnumerable<string> models, Func<string, TClient> createClient)
    where TClient : class
{
    private readonly List<string> models = models.ToList();
    private readonly Dictionary<int, TClient> clients = [];

    public TClient GetClient(float quality)
    {
        int index = models.GetPercentageIndex(quality);
        if (!clients.TryGetValue(index, out TClient? client))
        {
            client = createClient(models[index]);
            clients[index] = client;
        }

        return client;
    }
}