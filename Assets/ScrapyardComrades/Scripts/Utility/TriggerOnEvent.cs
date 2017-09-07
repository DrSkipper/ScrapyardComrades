using UnityEngine;

public class TriggerOnEvent : MonoBehaviour
{
    public string EventName;
    public MonoBehaviour Target;
    public string TargetMethodName;
    public int Delay;

    void Awake()
    {
        _delayTimer = new Timer(this.Delay, false, false, onDelayComplete);
        GlobalEvents.Notifier.Listen(this.EventName, this, execute);
    }

    void FixedUpdate()
    {
        _delayTimer.update();
    }

    /**
     * Private
     */
    private Timer _delayTimer;

    private void execute(LocalEventNotifier.Event e)
    {
        if (this.Delay > 0)
            _delayTimer.start();
        else
            onDelayComplete();
    }

    private void onDelayComplete()
    {
        if (!this.TargetMethodName.IsEmpty())
            this.Target.Invoke(this.TargetMethodName, 0.0f);
    }
}
