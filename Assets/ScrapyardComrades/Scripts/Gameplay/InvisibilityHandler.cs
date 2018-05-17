using UnityEngine;

public class InvisibilityHandler : VoBehavior, IPausable
{
    public float FadeSpeed = 0.05f;
    public bool DebugInvisibility = false; // Exposed for debugging

    public bool Invisible {
        get {
            return _invisible;
        }
        set {
            _invisible = value;
            updateInvisibility();
        }
    }

    void FixedUpdate()
    {
        if (_lerping)
        {
            if (Mathf.Abs(this.spriteRenderer.color.a - _targetAlpha) < 0.01f)
            {
                setA(_targetAlpha);
                _lerping = false;
            }
            else
            {
                setA(this.spriteRenderer.color.a.Approach(_targetAlpha, this.FadeSpeed));
            }
        }
    }

    /**
     * Private
     */
    private bool _invisible;
    private float _targetAlpha;
    private bool _lerping;

    private void updateInvisibility()
    {
        this.DebugInvisibility = _invisible;
        _targetAlpha = _invisible ? 0.0f : 1.0f;
        _lerping = true;
    }

    private void setA(float a)
    {
        Color c = this.spriteRenderer.color;
        c.a = a;
        this.spriteRenderer.color = c;
    }
}
