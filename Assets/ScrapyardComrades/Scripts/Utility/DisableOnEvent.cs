using UnityEngine;

public class DisableOnEvent : MonoBehaviour
{
    public bool ListenOnAwake = true;
    public string EventName = "START_PRESSED";
    public MonoBehaviour[] Behaviors;
    public bool Enable = false;

    void Awake()
    {
        if (this.ListenOnAwake)
            GlobalEvents.Notifier.Listen(this.EventName, this, onEvent);
    }

    void OnSpawn()
    {
        if (!this.ListenOnAwake)
            GlobalEvents.Notifier.Listen(this.EventName, this, onEvent);
    }

    void OnReturnToPool()
    {
        if (!this.ListenOnAwake)
            GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, this.EventName);
    }

    private void onEvent(LocalEventNotifier.Event e)
    {
        for (int i = 0; i < this.Behaviors.Length; ++i)
        {
            this.Behaviors[i].enabled = this.Enable;
        }
    }
}
