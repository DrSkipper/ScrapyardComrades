using UnityEngine;

public class HitEffectHandler : MonoBehaviour, IPausable
{
    public SCSpriteAnimator Animator;

    public void InitializeWithFreezeFrames(int freezeFrames, SCSpriteAnimation animation, int dir)
    {
        _animStarted = false;
        this.Animator.PlayAnimation(animation);
        this.Animator.Stop();
        _freezeTimer = new Timer(freezeFrames, false, true);

        if (dir == 0)
            dir = Random.Range(0, 2) == 0 ? -1 : 1;
        else
            dir = Mathf.Clamp(dir, -1, 1);

        this.transform.localScale = new Vector3(dir * Mathf.Abs(this.transform.localScale.x), this.transform.localScale.y, this.transform.localScale.z);
    }

    void FixedUpdate()
    {
        if (!_animStarted)
        {
            _freezeTimer.update();
            if (_freezeTimer.Completed)
            {
                _animStarted = true;
                this.Animator.Play();
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
