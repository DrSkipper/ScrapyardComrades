using UnityEngine;

public class SimpleParticle : VoBehavior
{
    public SCSpriteAnimator Animator;

    public void Emit(SCSpriteAnimation animation, AbstractParticleEmitter.OnCompleteDelegate onComplete)
    {
        this.Animator.PlayAnimation(animation);
        _onComplete = onComplete;
    }

    protected virtual void FixedUpdate()
    {
        if (!this.Animator.IsPlaying)
            _onComplete(this.gameObject);
    }

    /**
     * Private
     */
    private AbstractParticleEmitter.OnCompleteDelegate _onComplete;
}
