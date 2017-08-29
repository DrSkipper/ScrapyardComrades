using UnityEngine;

public class SimpleParticle : VoBehavior
{
    public SCSpriteAnimator Animator;
    public delegate void OnCompleteDelegate(SimpleParticle particle);

    public void Emit(SCSpriteAnimation animation, OnCompleteDelegate onComplete)
    {
        this.Animator.PlayAnimation(animation);
        _onComplete = onComplete;
    }

    void FixedUpdate()
    {
        if (!this.Animator.IsPlaying)
            _onComplete(this);
    }

    /**
     * Private
     */
    private OnCompleteDelegate _onComplete;
}
