using UnityEngine;

public class TriggerOnEvent : MonoBehaviour
{
    public string EventName;
    public MonoBehaviour Target;
    public string TargetMethodName;

    void Awake()
    {
        GlobalEvents.Notifier.Listen(this.EventName, this, execute);
    }

    /**
     * Private
     */
    private void execute(LocalEventNotifier.Event e)
    {
        this.Target.Invoke(this.TargetMethodName, 0.0f);
    }
}
