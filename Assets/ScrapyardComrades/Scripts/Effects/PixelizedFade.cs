using UnityEngine;

public class PixelizedFade : MonoBehaviour, IPausable
{
    public PixelizeScreenEffect ScreenEffect;
    public int Duration;
    public float InitialMultiplier;
    public float FinalMultiplier;
    public Easing.Flow FadeFlow;
    public Easing.Flow ReverseFlow = Easing.Flow.Out;
    public Easing.Function FadeFunction;

    public bool Completed { get { return _t >= this.Duration; } }

    void Awake()
    {
        _easingDelegate = Easing.GetFunction(this.FadeFunction, this.FadeFlow);
        _reverseDelegate = Easing.GetFunction(this.FadeFunction, this.ReverseFlow);
    }

    void FixedUpdate()
    {
        if (!_reverse)
            updateForward();
        else
            updateReverse();
    }

    public void Reverse()
    {
        _reverse = !_reverse;
        _t = 0;
        this.enabled = true;
        this.ScreenEffect.ResolutionMultiplier = _reverse ? this.FinalMultiplier : this.InitialMultiplier;

        if (SystemInfo.supportsImageEffects)
            this.ScreenEffect.enabled = true;
    }

    /**
     * Private
     */
    private Easing.EasingDelegate _easingDelegate;
    private Easing.EasingDelegate _reverseDelegate;
    private int _t;
    private bool _reverse;

    private void updateForward()
    {
        if (_t <= this.Duration)
        {
            if (_t == this.Duration)
            {
                this.ScreenEffect.ResolutionMultiplier = this.FinalMultiplier;
                this.enabled = false;
                this.ScreenEffect.enabled = false;
            }
            else
            {
                this.ScreenEffect.ResolutionMultiplier = _easingDelegate(_t, this.InitialMultiplier, this.FinalMultiplier - this.InitialMultiplier, this.Duration);
                ++_t;
            }
        }
    }

    private void updateReverse()
    {
        if (_t <= this.Duration)
        {
            if (_t == this.Duration)
            {
                this.ScreenEffect.ResolutionMultiplier = this.InitialMultiplier;
                this.enabled = false;
                //this.ScreenEffect.enabled = false;
            }
            else
            {
                this.ScreenEffect.ResolutionMultiplier = _reverseDelegate(_t, this.FinalMultiplier, this.InitialMultiplier - this.FinalMultiplier, this.Duration);
                ++_t;
            }
        }
    }
}
