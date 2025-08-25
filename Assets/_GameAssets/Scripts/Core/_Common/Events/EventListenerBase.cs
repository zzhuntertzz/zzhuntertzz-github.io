using System.Collections.Generic;
using TigerForge;
using UnityEngine;
using UnityEngine.Events;

public abstract class EventListenerBase : MonoBehaviour
{
    protected virtual Dictionary<string, UnityAction> GetListEvents()
    {
        return new();
    }
    
    protected virtual void StartListeners()
    {
        foreach (var eventKey in GetListEvents())
        {
            StartListener(eventKey.Key, eventKey.Value);
        }
    }
    
    protected void StartListener(string key, UnityAction action)
    {
        EventManager.StartListening(key, action);
    }

    protected virtual void StopListeners()
    {
        foreach (var eventKey in GetListEvents())
        {
            StopListener(eventKey.Key, eventKey.Value);
        }
    }
    
    protected void StopListener(string key, UnityAction action)
    {
        EventManager.StopListening(key, action);
    }
}
