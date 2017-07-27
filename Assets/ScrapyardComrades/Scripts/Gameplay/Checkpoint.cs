using UnityEngine;

public class Checkpoint : VoBehavior, IPausable
{
    public const string CHECKPOINT_STATE = "checkpoint";
    public const string BROKEN_STATE = "broken";

    public WorldEntity WorldEntity;
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
    public SCSpriteAnimation BrokenIdleAnim;

    void OnSpawn()
    {
        this.TopCollider.AddToCollisionPool();
        this.LightsOff.sortingOrder = this.spriteRenderer.sortingOrder;
        this.LightsOn.sortingOrder = this.spriteRenderer.sortingOrder;
        
        bool active = (SaveData.CheckGlobalState(CHECKPOINT_STATE, this.WorldEntity.QuadName));

        if (active && EntityTracker.Instance.IsInitialLoad)
        {
            _broken = true;
            this.WorldEntity.StateTag = BROKEN_STATE;
        }
        else
        {
            _broken = this.WorldEntity.StateTag == BROKEN_STATE;
        }

        // If this checkpoint is broken, break it
        if (_broken)
        {
            this.Animator.PlayAnimation(this.BrokenIdleAnim);
        }
        else
        {
            // If the saved checkpoint is in this quad, activate
            setActive(active);
            this.Animator.PlayAnimation(active ? this.ActiveIdleAnim : this.InactiveIdleAnim);

            GlobalEvents.Notifier.Listen(CheckpointSetEvent.NAME, this, onCheckpointSet);
        }
    }

    void Update()
    {
        if (!_broken)
        {
            if (!_active)
            {
                //TODO: Probably want to save to disk if you pass by this checkpoint even when it's already activated
                GameObject collided = this.RangeCollider.CollideFirst(0, 0, this.ActivatorMask);
                if (collided != null)
                    activate(collided.GetComponent<Damagable>());
            }
            else if (_activeCycles < 3)
            {
                if (!this.Animator.IsPlaying)
                    incrementActivatedCycle();
            }

        }
    }

    void OnReturnToPool()
    {
        _broken = false;
        _active = false;
        this.TopCollider.RemoveFromCollisionPool();
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, CheckpointSetEvent.NAME);
    }

    /**
     * Private
     */
    private bool _broken;
    private bool _active;
    private int _activeCycles;

    private void activate(Damagable collided)
    {
        SaveData.PlayerStats.CurrentHealth = collided.Health;
        SaveData.PlayerStats.MaxHealth = collided.MaxHealth;
        SaveData.PlayerStats.Level = collided.GetComponent<PlayerHealthController>().HeroLevel;
        SaveData.UnsafeSave = false;
        SaveData.SetGlobalState(CHECKPOINT_STATE, this.WorldEntity.QuadName);
        GlobalEvents.Notifier.SendEvent(new CheckpointSetEvent(this.WorldEntity.QuadName), true);
        SaveData.SaveToDisk();
        setActive(true);
        this.Animator.PlayAnimation(this.ActivateAnim);
    }

    private void deactivate()
    {
        setActive(false);
        this.Animator.PlayAnimation(this.InactiveIdleAnim);
    }

    private void setActive(bool active)
    {
        _active = active;
        _activeCycles = 0;
        this.LightsOff.gameObject.SetActive(!active);
        this.LightsOn.gameObject.SetActive(active);
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

    private void onCheckpointSet(LocalEventNotifier.Event e)
    {
        if ((e as CheckpointSetEvent).QuadName != this.WorldEntity.QuadName)
            deactivate();
    }
}
