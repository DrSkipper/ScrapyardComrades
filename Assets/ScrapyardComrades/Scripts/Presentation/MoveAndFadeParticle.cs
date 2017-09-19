using UnityEngine;

public class MoveAndFadeParticle : VoBehavior
{
    public IntegerVector VelocityDir;
    public float MinVelocityMultiplier = 1.0f;
    public float MaxVelocityMultiplier = 1.0f;
    public int FadeDelay = 25;
    public float FadeSpeed = 0.05f;

    public void Emit(AbstractParticleEmitter.OnCompleteDelegate onComplete)
    {
        _actualPos = this.transform.position;
        _velocity = ((Vector2)this.VelocityDir) * Random.Range(this.MinVelocityMultiplier, this.MaxVelocityMultiplier);
        _t = this.FadeDelay;

        Color c = this.spriteRenderer.color;
        c.a = 1.0f;
        this.spriteRenderer.color = c;
        _onComplete = onComplete;
    }

    protected void FixedUpdate()
    {
        _actualPos += _velocity;
        this.transform.SetPosition2D(Mathf.RoundToInt(_actualPos.x), Mathf.RoundToInt(_actualPos.y));

        if (_t > 0)
        {
            --_t;
        }
        else
        {
            Color c = this.spriteRenderer.color;
            c.a -= this.FadeSpeed;
            if (c.a <= 0.0f)
                _onComplete(this.gameObject);
            else
                this.spriteRenderer.color = c;
        }
    }

    /**
     * Private
     */
    private AbstractParticleEmitter.OnCompleteDelegate _onComplete;
    private Vector2 _actualPos;
    private Vector2 _velocity;
    private int _t;
}
