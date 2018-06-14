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
    public SoundData.Key BubbleKey1;
    public SoundData.Key BubbleKey2;
    public SoundData.Key BubbleKey3;
    public SoundData.Key BubbleKey4;
    public int BubbleInterval1 = 32;
    public int BubbleInterval2 = 22;
    public int BubbleInterval3 = 15;
    public int BubbleInterval4 = 10;

    void Awake()
    {
        _bubbleTimer1 = new Timer(this.BubbleInterval1, true, false, playBubbleSound1);
        _bubbleTimer2 = new Timer(this.BubbleInterval2, true, false, playBubbleSound2);
        _bubbleTimer3 = new Timer(this.BubbleInterval3, true, false, playBubbleSound3);
        _bubbleTimer4 = new Timer(this.BubbleInterval4, true, false, playBubbleSound4);
    }

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
            stopTimers();
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
            _bubbleTimer1.update();
            _bubbleTimer2.update();
            _bubbleTimer3.update();
            _bubbleTimer4.update();

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
        stopTimers();
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
    private Timer _bubbleTimer1;
    private Timer _bubbleTimer2;
    private Timer _bubbleTimer3;
    private Timer _bubbleTimer4;

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
        if (!active)
            stopTimers();
        _active = active;
        _activeCycles = 0;
        this.LightsOff.gameObject.SetActive(!active);
        this.LightsOn.gameObject.SetActive(active);
    }

    private void stopTimers()
    {
        _bubbleTimer1.Paused = true;
        _bubbleTimer2.Paused = true;
        _bubbleTimer3.Paused = true;
        _bubbleTimer4.Paused = true;
    }

    private void startTimers()
    {
        _bubbleTimer1.resetAndStart();
        _bubbleTimer2.resetAndStart();
        _bubbleTimer3.resetAndStart();
        _bubbleTimer4.resetAndStart();
    }

    private void incrementActivatedCycle()
    {
        if (_activeCycles == 0)
            this.Animator.PlayAnimation(this.FirstActiveAnim);
        else if (_activeCycles == 1)
        {
            this.Animator.PlayAnimation(this.SecondActiveAnim);
            startTimers();
        }
        else
            this.Animator.PlayAnimation(this.ActiveIdleAnim);

        ++_activeCycles;
    }

    private void onCheckpointSet(LocalEventNotifier.Event e)
    {
        if ((e as CheckpointSetEvent).QuadName != this.WorldEntity.QuadName)
            deactivate();
    }

    private void playBubbleSound1() { SoundManager.Play(this.BubbleKey1, this.transform); }
    private void playBubbleSound2() { SoundManager.Play(this.BubbleKey2, this.transform); }
    private void playBubbleSound3() { SoundManager.Play(this.BubbleKey3, this.transform); }
    private void playBubbleSound4() { SoundManager.Play(this.BubbleKey4, this.transform); }
}
