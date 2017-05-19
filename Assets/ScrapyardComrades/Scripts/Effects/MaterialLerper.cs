using UnityEngine;

public class MaterialLerper : MonoBehaviour, IPausable
{
    public Material Material;
    public string PropertyName = "_LerpAmount";
    public float InitialValue = 1.0f;
    public float FinalValue = 0.0f;
    public float Duration = 1.0f;
    public Easing.Flow EasingFlow;
    public Easing.Function EasingFunction;
    public bool IsLerping = true;
    public OnCompleteCallback OnComplete;

    public delegate void OnCompleteCallback();

    void Awake()
    {
        _easingDelegate = Easing.GetFunction(this.EasingFunction, this.EasingFlow);
    }

    public void BeginLerp()
    {
        _t = 0.0f;
        this.Material.SetFloat(this.PropertyName, this.InitialValue);
        this.IsLerping = true;
    }

    void Update()
    {
        if (this.IsLerping)
        {
            if (_t >= this.Duration)
            {
                this.Material.SetFloat(this.PropertyName, this.FinalValue);
                this.IsLerping = false;

                if (this.OnComplete != null)
                    this.OnComplete();
            }
            else
            {
                this.Material.SetFloat(this.PropertyName, _easingDelegate(_t, this.InitialValue, this.FinalValue - this.InitialValue, this.Duration));

                _t += Time.deltaTime;
            }
        }
    }

    /**
     * Private
     */
    private Easing.EasingDelegate _easingDelegate;
    private float _t;
}
