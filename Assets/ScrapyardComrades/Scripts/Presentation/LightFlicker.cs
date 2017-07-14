using UnityEngine;

public class LightFlicker : MonoBehaviour, IPausable
{
    public Light Light;
    public IntegerVector[] OnDurationRanges;
    public IntegerVector OffDurationRange;

    void OnSpawn()
    {
        if (_timer == null)
            _timer = new Timer(1, true, false, timerCompleted);

        turnOn();
    }

    void FixedUpdate()
    {
        _timer.update();
    }

    /**
     * Private
     */
    private Timer _timer;
    private bool _on;

    private void timerCompleted()
    {
        if (_on)
            turnOff();
        else
            turnOn();
    }

    private void turnOn()
    {
        _on = true;
        this.Light.enabled = true;
        resetTimer(this.OnDurationRanges[Random.Range(0, this.OnDurationRanges.Length)]);
    }

    private void turnOff()
    {
        _on = false;
        this.Light.enabled = false;
        resetTimer(this.OffDurationRange);
    }

    private void resetTimer(IntegerVector range)
    {
        _timer.reset(Random.Range(range.X, range.Y + 1));
        _timer.start();
    }
}
