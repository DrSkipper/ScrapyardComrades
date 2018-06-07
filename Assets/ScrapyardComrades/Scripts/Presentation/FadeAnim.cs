using UnityEngine;

public class FadeAnim : VoBehavior, IPausable
{
    public SCSpriteAnimator Animator;
    public int FrameToStart = 0;
    public float FadePerFrame = 0.1f;
    
    void OnSpawn()
    {
        this.spriteRenderer.SetAlpha(1.0f);
    }

    void FixedUpdate()
    {
        if (this.Animator.Elapsed >= this.FrameToStart)
        {
            this.spriteRenderer.AddAlpha(-this.FadePerFrame);
        }
    }
}
