using UnityEngine;

public class PixelizedFade : MonoBehaviour, IPausable
{
    public PixelizeScreenEffect ScreenEffect;
    public float Duration;
    public float InitialMultiplier;
    public float FinalMultiplier;
    public Easing.Flow FadeFlow;
    public Easing.Function FadeFunction;

    void Awake()
    {
        _easingDelegate = Easing.GetFunction(this.FadeFunction, this.FadeFlow);
    }

    void FixedUpdate()
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

    /**
     * Private
     */
    private Easing.EasingDelegate _easingDelegate;
    private int _t;
}
