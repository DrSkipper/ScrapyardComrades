using UnityEngine;
using System.Collections.Generic;

public class LocalEventNotifier : MonoBehaviour
{
    public delegate void EventCallback(Event localEvent);

    public class Event
    {
        public string Name;
    }

    public void Listen(string eventName, MonoBehaviour owner, EventCallback callback)
    {
        if (_listenersByEventName == null)
            _listenersByEventName = new Dictionary<string, List<Listener>>();

        List<Listener> listeners = null;
        if (_listenersByEventName.ContainsKey(eventName))
        {
            listeners = _listenersByEventName[eventName];
        }
        else
        {
            listeners = new List<Listener>();
            _listenersByEventName[eventName] = listeners;
        }

        listeners.Add(new Listener(owner, callback));
    }

    public void RemoveAllListenersForOwner(MonoBehaviour owner)
    {
        if (_listenersByEventName == null)
            return;

        foreach (List<Listener> listeners in _listenersByEventName.Values)
        {
            listeners.RemoveAll(listener => listener.Owner == owner);
        }
    }

    public void SendEvent(Event localEvent)
    {
        if (_listenersByEventName == null)
            return;
        
        if (_listenersByEventName.ContainsKey(localEvent.Name))
        {
            foreach (Listener listener in _listenersByEventName[localEvent.Name])
            {
                listener.Callback(localEvent);
            }
        }
    }

    /**
     * Private
     */
    private Dictionary<string, List<Listener>> _listenersByEventName;

    private class Listener
    {
        public MonoBehaviour Owner;
        public EventCallback Callback;

        public Listener(MonoBehaviour owner, EventCallback callback)
        {
            this.Owner = owner;
            this.Callback = callback;
        }
    }
}
