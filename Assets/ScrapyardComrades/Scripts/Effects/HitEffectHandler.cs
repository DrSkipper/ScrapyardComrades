using UnityEngine;

public class HitEffectHandler : MonoBehaviour
{
    public Animator Animator;

    public void InitializeWithFreezeFrames(int freezeFrames)
    {
        this.Animator.Play("Empty");
        _freezeTimer = new Timer(freezeFrames, false, true);
    }

    void FixedUpdate()
    {
        if (_freezeTimer != null)
        {
            _freezeTimer.update();
            if (_freezeTimer.Completed)
                this.Animator.SetTrigger("Begin");
        }
    }

    public void EffectCompleted()
    {
        ObjectPools.Release(this.gameObject);
    }

    private Timer _freezeTimer;
}
