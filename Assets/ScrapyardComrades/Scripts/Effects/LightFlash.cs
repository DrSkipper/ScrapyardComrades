using UnityEngine;

public class LightFlash : MonoBehaviour, IPausable
{
    public Light Light;
    public int Duration = 15;

    [Header("Range Flow")]
    public bool HorseshoeRange = false;
    public Easing.Function RangeEasingFunction;
    public Easing.Flow RangeEasingFlow;
    public float StartRange = 10.0f;
    public float EndRange = 100.0f;

    [Header("Intensity Flow")]
    public bool HorseshoeIntensity = true;
    public Easing.Function IntensityEasingFunction;
    public Easing.Flow IntensityEasingFlow;
    public float StartIntensity = 0.0f;
    public float EndIntensity = 5.0f;

    void OnSpawn()
    {
        _t = 0;
        _rangeDelegate = Easing.GetFunction(this.RangeEasingFunction, this.RangeEasingFlow);
        _intensityDelegate = Easing.GetFunction(this.IntensityEasingFunction, this.IntensityEasingFlow);
        _horseshoeDuration = this.Duration / 2;

        this.Light.range = this.StartRange;
        this.Light.intensity = this.StartIntensity;
    }

    public void StartWithDelayAndColor(int delay, Color color)
    {
        this.Light.color = color;
        if (delay <= 0)
            return;

        if (_delayTimer == null)
        {
            _delayTimer = new Timer(delay, false, true);
        }
        else
        {
            _delayTimer.reset(delay);
            _delayTimer.start();
        }
    }

    void FixedUpdate()
    {
        if (_delayTimer != null)
        {
            _delayTimer.update();
            if (!_delayTimer.Completed)
                return;
        }

        bool horseshoe = _t >= _horseshoeDuration;
        bool hsRange = horseshoe && this.HorseshoeRange;
        bool hsIntensity = horseshoe && this.HorseshoeIntensity;

        float initialRange = hsRange ? this.EndRange : this.StartRange;
        float targetRange = hsRange ? this.StartRange : this.EndRange;
        float initialIntensity = hsIntensity ? this.EndIntensity : this.StartIntensity;
        float targetIntensity = hsIntensity ? this.StartIntensity : this.EndIntensity;

        this.Light.range = _rangeDelegate(hsRange ? _t - _horseshoeDuration :_t, initialRange, targetRange - initialRange, this.HorseshoeRange ? _horseshoeDuration : this.Duration);
        this.Light.intensity = _intensityDelegate(hsIntensity ? _t - _horseshoeDuration : _t, initialIntensity, targetIntensity - initialIntensity, this.HorseshoeIntensity ? _horseshoeDuration : this.Duration);

        ++_t;

        if (_t > this.Duration)
            ObjectPools.Release(this.gameObject);
    }

    /**
     * Private
     */
    private Easing.EasingDelegate _rangeDelegate;
    private Easing.EasingDelegate _intensityDelegate;
    private int _t;
    private int _horseshoeDuration;
    private Timer _delayTimer;
}
