using UnityEngine;

public class HitEffectHandler : MonoBehaviour
{
    public Animator Animator;

    public void InitializeWithFreezeFrames(int freezeFrames)
    {
        _freezeTimer = new Timer(freezeFrames, false, true);
    }

    void FixedUpdate()
    {
        if (_freezeTimer != null)
        {
            _freezeTimer.update();
            if (_freezeTimer.Completed)
                this.Animator.SetTrigger(BEGIN_TRIGGER);
        }
    }

    public void EffectCompleted()
    {
        ObjectPools.Release(this.gameObject);
    }

    void OnReturnToPool()
    {
        this.Animator.Play(STARTING_STATE);
    }

    private Timer _freezeTimer;
    private const string STARTING_STATE = "Empty";
    private const string BEGIN_TRIGGER = "Begin";
}
