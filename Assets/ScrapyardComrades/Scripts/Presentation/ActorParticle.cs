using UnityEngine;

public class ActorParticle : Actor2D, AbstractParticle
{
    public SCSpriteAnimator Animator;
    public Vector2 XVelocityRange;
    public Vector2 YVelocityRange;
    public bool UseLocalXForSign = false;
    public bool UseLocalYForSign = false;
    public bool RandomFlipX = false;
    public bool RandomFlipY = false;
    public float Gravity = 0.0f;

    public void Emit(SCSpriteAnimation animation, AbstractParticleEmitter.OnCompleteDelegate onComplete)
    {
        this.Animator.PlayAnimation(animation);
        _onComplete = onComplete;

        this.Velocity = new Vector2(Random.Range(this.XVelocityRange.x, this.XVelocityRange.y), Random.Range(this.YVelocityRange.x, this.YVelocityRange.y));
        if (this.UseLocalXForSign)
            this.Velocity.x = Mathf.Sign(this.transform.localPosition.x) * this.Velocity.x;
        if (this.UseLocalYForSign)
            this.Velocity.y = Mathf.Sign(this.transform.localPosition.y) * this.Velocity.y;
        if (this.RandomFlipX && Random.value > 0.5f)
            this.Velocity.x = -this.Velocity.x;
        if (this.RandomFlipY && Random.value > 0.5f)
            this.Velocity.y = -this.Velocity.y;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        this.Velocity.y -= this.Gravity;

        if (!this.Animator.IsPlaying)
            _onComplete(this.gameObject);
    }

    /**
     * Private
     */
    private AbstractParticleEmitter.OnCompleteDelegate _onComplete;
}
