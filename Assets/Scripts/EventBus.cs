using R3;

public class EventBus
{
    private static readonly Subject<string> eventBus = new();

    public static void Publish(string key) => eventBus.OnNext(key);

    public static Observable<string> OnEvent(string key)
    {
        return eventBus.Where(k => k == key);
    }
}