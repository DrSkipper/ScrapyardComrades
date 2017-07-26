using UnityEngine;

public class Checkpoint : VoBehavior, IPausable
{
    public LayerMask ActivatorMask;
    public IntegerCollider RangeCollider;
    public IntegerCollider TopCollider;
    public SpriteRenderer LightsOff;
    public SpriteRenderer LightsOn;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation InactiveIdleAnim;
    public SCSpriteAnimation ActivateAnim;
    public SCSpriteAnimation FirstActiveAnim;
    public SCSpriteAnimation SecondActiveAnim;
    public SCSpriteAnimation ActiveIdleAnim;

    void OnSpawn()
    {
        this.TopCollider.AddToCollisionPool();
        this.LightsOff.sortingOrder = this.spriteRenderer.sortingOrder;
        this.LightsOn.sortingOrder = this.spriteRenderer.sortingOrder;

        //TODO: data-driven
        deactivate();
    }

    void Update()
    {
        if (!_active)
        {
            if (this.RangeCollider.CollideFirst(0, 0, this.ActivatorMask) != null)
                activate();
        }
        else if (_activeCycles < 3)
        {
            if (!this.Animator.IsPlaying)
                incrementActivatedCycle();
        }
    }

    void OnReturnToPool()
    {
        this.TopCollider.RemoveFromCollisionPool();
    }

    /**
     * Private
     */
    private bool _active;
    private int _activeCycles;

    private void activate()
    {
        _active = true;
        this.Animator.PlayAnimation(this.ActivateAnim);
        this.LightsOff.gameObject.SetActive(false);
        this.LightsOn.gameObject.SetActive(true);
    }

    private void deactivate()
    {
        _active = false;
        _activeCycles = 0;
        this.Animator.PlayAnimation(this.InactiveIdleAnim);
        this.LightsOff.gameObject.SetActive(true);
        this.LightsOn.gameObject.SetActive(false);
    }

    private void incrementActivatedCycle()
    {
        if (_activeCycles == 0)
            this.Animator.PlayAnimation(this.FirstActiveAnim);
        else if (_activeCycles == 1)
            this.Animator.PlayAnimation(this.SecondActiveAnim);
        else
            this.Animator.PlayAnimation(this.ActiveIdleAnim);

        ++_activeCycles;
    }
}
