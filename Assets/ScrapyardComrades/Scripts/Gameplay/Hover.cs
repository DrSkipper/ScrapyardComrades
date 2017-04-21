using UnityEngine;

public class Hover : MonoBehaviour
{
    public int Range = 10;
    public int OneWayDurationFrames = 40;
    public Easing.Function EasingFunction;
    public Easing.Flow EasingFlow;

    void OnSpawn()
    {
        _min = this.transform.position;
        _max = new Vector2(this.transform.position.x, this.transform.position.y + this.Range);
        _goingUp = true;
        _t = 0;
        _easingDelegate = Easing.GetFunction(this.EasingFunction, this.EasingFlow);

        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    void FixedUpdate()
    {
        Vector2 newPos = (_goingUp ? _min : _max).EaseTowards(_goingUp ? _max : _min, _t, this.OneWayDurationFrames, _easingDelegate);
        this.transform.SetPosition2D(newPos.x, newPos.y);
        
        if (_t == this.OneWayDurationFrames)
        {
            _goingUp = !_goingUp;
            _t = 0;
        }
        else
        {
            ++_t;
        }
    }

    void OnReturnToPool()
    {
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, WorldRecenterEvent.NAME);
    }

    /**
     * Private
     */
    private int _t;
    private bool _goingUp;
    private Vector2 _min;
    private Vector2 _max;
    private Easing.EasingDelegate _easingDelegate;

    private void onRecenter(LocalEventNotifier.Event e)
    {
        Vector2 offset = (e as WorldRecenterEvent).RecenterOffset;
        _min += offset;
        _max += offset;
    }
}
