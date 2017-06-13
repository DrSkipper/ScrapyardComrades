using UnityEngine;
using UnityEngine.UI;

public class TextBlinker : MonoBehaviour, IPausable
{
    public Text Text;
    public float MinAlpha;
    public float MaxAlpha;
    public int DurationAtMax;
    public int DurationAtMin;
    public int FadeDuration;
    public Easing.Flow FadeOutFlow;
    public Easing.Flow FadeInFlow;
    public Easing.Function FadeFunction;

    void Awake()
    {
        _stateTimer = new Timer(this.DurationAtMin, false, true, stateFinished);
        _fadeOutEasing = Easing.GetFunction(this.FadeFunction, this.FadeOutFlow);
        _fadeInEasing = Easing.GetFunction(this.FadeFunction, this.FadeInFlow);
        _fading = false;
        _fadingOut = true;
        _distance = this.MaxAlpha - this.MinAlpha;
    }

    void FixedUpdate()
    {
        if (!_fading)
        {
            _stateTimer.update();
        }
        else
        {
            Color c = this.Text.color;

            if (_fadingOut)
            {
                c.a = _fadeOutEasing(_fadeT, this.MaxAlpha, -_distance, this.FadeDuration);
            }
            else
            {
                c.a = _fadeInEasing(_fadeT, this.MinAlpha, _distance, this.FadeDuration);
            }

            this.Text.color = c;

            if (_fadeT >= this.FadeDuration)
            {
                _fading = false;
                _stateTimer.reset(_fadingOut ? this.DurationAtMin : this.DurationAtMax);
                _stateTimer.start();
            }
            else
            {
                ++_fadeT;
            }
        }
    }

    public void TightenDurations()
    {
        _fadeT = this.FadeDuration;

        this.DurationAtMax /= 16;
        this.DurationAtMin /= 16;
        this.FadeDuration /= 16;

        if (!_stateTimer.Completed)
            _stateTimer.complete();
    }

    /**
     * Private
     */
    private Timer _stateTimer;
    private Easing.EasingDelegate _fadeOutEasing;
    private Easing.EasingDelegate _fadeInEasing;
    private bool _fading;
    private bool _fadingOut;
    private int _fadeT;
    private float _distance;

    private void stateFinished()
    {
        _fadingOut = !_fadingOut;
        _fading = true;
        _fadeT = 0;
    }
}
