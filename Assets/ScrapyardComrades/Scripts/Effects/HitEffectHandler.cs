using UnityEngine;

public class HitEffectHandler : MonoBehaviour, IPausable
{
    public SCSpriteAnimator Animator;

    public void InitializeWithFreezeFrames(int freezeFrames)
    {
        _animStarted = false;
        this.Animator.PlayAnimation(this.Animator.DefaultAnimation);
        this.Animator.Stop();
        _freezeTimer = new Timer(freezeFrames, false, true);
    }

    void FixedUpdate()
    {
        if (!_animStarted)
        {
            _freezeTimer.update();
            if (_freezeTimer.Completed)
            {
                _animStarted = true;
                this.Animator.PlayAnimation(this.Animator.DefaultAnimation);
            }
        }
        else
        {
            if (!this.Animator.IsPlaying)
            {
                _animStarted = false;
                ObjectPools.Release(this.gameObject);
            }
        }
    }

    private bool _animStarted;
    private Timer _freezeTimer;
    private const string STARTING_STATE = "Empty";
    private const string BEGIN_TRIGGER = "Begin";
}
