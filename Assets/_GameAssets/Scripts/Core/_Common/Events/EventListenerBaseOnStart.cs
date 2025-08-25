public abstract class EventListenerBaseOnStart : EventListenerBase
{
    protected virtual void Start()
    {
        StartListeners();
    }

    protected virtual void OnDestroy()
    {
        StopListeners();
    }
}
