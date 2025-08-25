public abstract class EventListenerBaseOnEnable : EventListenerBase
{
    protected virtual void OnEnable()
    {
        StartListeners();
    }

    protected virtual void OnDisable()
    {
        StopListeners();
    }
}
