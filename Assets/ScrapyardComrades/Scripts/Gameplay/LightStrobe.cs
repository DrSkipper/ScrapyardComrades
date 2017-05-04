using UnityEngine;

public class LightStrobe : MonoBehaviour
{
    public Light Light;
    public int RangeRange = 20;
    public float IntensityRange = 3;
    public int OneWayDuration = 50;
    public Easing.Function EasingFunction;
    public Easing.Flow EasingFlow;

    void OnSpawn()
    {
        _maxRange = Mathf.RoundToInt(this.Light.range) + this.RangeRange / 2;
        _minRange = _maxRange - this.RangeRange;
        _maxIntensity = this.Light.intensity + this.IntensityRange / 2.0f;
        _minIntensity = _maxIntensity - this.IntensityRange;
        _t = this.OneWayDuration / 2;
        _goingUp = false;
        _easingDelegate = Easing.GetFunction(this.EasingFunction, this.EasingFlow);
    }

    void FixedUpdate()
    {
        if (_goingUp)
        {
            this.Light.range =  _easingDelegate(_t, _minRange, this.RangeRange, this.OneWayDuration);
            this.Light.intensity = _easingDelegate(_t, _minIntensity, this.IntensityRange, this.OneWayDuration);
        }
        else
        {
            this.Light.range = _easingDelegate(_t, _maxRange, -this.RangeRange, this.OneWayDuration);
            this.Light.intensity = _easingDelegate(_t, _maxIntensity, -this.IntensityRange, this.OneWayDuration);
        }

        if (_t == this.OneWayDuration)
        {
            _goingUp = !_goingUp;
            _t = 0;
        }
        else
        {
            ++_t;
        }
    }

    /**
     * Private
     */
    private bool _goingUp;
    private int _t;
    private Easing.EasingDelegate _easingDelegate;
    private int _minRange;
    private int _maxRange;
    private float _minIntensity;
    private float _maxIntensity;
}
