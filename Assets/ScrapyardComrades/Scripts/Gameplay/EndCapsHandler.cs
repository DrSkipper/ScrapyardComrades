using UnityEngine;

public class EndCapsHandler : VoBehavior
{
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation MiddleAnim;
    public SCSpriteAnimation LeftAnim;
    public SCSpriteAnimation RightAnim;

    void OnSpawn()
    {
        _hasConfigured = false;
    }

    void Update()
    {
        if (!_hasConfigured)
        {
            _hasConfigured = true;

            bool right = this.integerCollider.CollideFirst(1, 0, this.layerMask) != null;
            bool left = this.integerCollider.CollideFirst(-1, 0, this.layerMask) != null;

            if (right && left)
                this.Animator.PlayAnimation(this.MiddleAnim);
            else if (right)
                this.Animator.PlayAnimation(this.LeftAnim);
            else
                this.Animator.PlayAnimation(this.RightAnim);
        }
    }

    /**
     * Private
     */
    private bool _hasConfigured;
}
