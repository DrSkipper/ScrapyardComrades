using UnityEngine;

public class PowerupObject : VoBehavior, IPausable
{
    public SCSpriteAnimator Animator;
    public SCSpriteAnimation IdleAnimation;
    public SCSpriteAnimation PickupAnimation;
    public IntegerCollider Collider;
    public LayerMask ConsumerMask;
    public int VerticalSpeed = 1;
    public float Gravity = 1.0f;

    public bool ThrowSpawnAnimation = true;
    public string ThrowOnEvent = "TRIGGER";
    public Vector2 ThrowSpawnVelocity;
    public float ThrowBounceMultiplier = 0.5f;
    public int ThrowScaleDuration = 35;

    void OnSpawn()
    {
        if (_destructionTimer == null)
            _destructionTimer = new Timer(this.PickupAnimation.LengthInFrames, false, false, onDestruction);
        else
            _destructionTimer.Paused = true;

        this.Actor.enabled = false;

        if (this.ThrowSpawnAnimation)
        {
            if (_throwScaleTimer == null)
            {
                _throwScaleTimer = new Timer(this.ThrowScaleDuration, false, true);
            }
            else
            {
                _throwScaleTimer.reset(this.ThrowScaleDuration);
                _throwScaleTimer.start();
            }

            GlobalEvents.Notifier.Listen(this.ThrowOnEvent, this, throwCoin);
            this.transform.SetScale2D(0, 0);
        }
        else
        {
            this.transform.SetScale2D(1, 1);
        }
    }

    void OnReturnToPool()
    {
        if (this.ThrowSpawnAnimation)
            GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, this.ThrowOnEvent);
    }

    void FixedUpdate()
    {
        if (_destructionTimer.IsRunning)
        {
            _destructionTimer.update();
            this.transform.SetY(this.transform.position.y + this.VerticalSpeed);
        }
        else
        {
            if (_throwing)
            {
                VelocityModifier vMod = this.Actor.GetVelocityModifier(THROW_VMOD);
                vMod.Modifier = new Vector2(vMod.Modifier.x, vMod.Modifier.y - this.Gravity);

                _throwScaleTimer.update();
                float s = 1.0f - (float)_throwScaleTimer.FramesRemaining / (float)this.ThrowScaleDuration;
                s = 0.2f + s * 0.8f;
                this.transform.SetScale2D(s, s);
            }

            if (_throwScaleTimer == null || _throwScaleTimer.Completed)
            {
                GameObject collided = this.Collider.CollideFirst(0, 0, this.ConsumerMask);
                if (collided != null)
                {
                    PowerupConsumer consumer = collided.GetComponent<PowerupConsumer>();
                    if (consumer != null)
                    {
                        _throwing = false;
                        this.Actor.enabled = false;
                        consumer.ConsumePowerup();
                        this.Animator.PlayAnimation(this.PickupAnimation);
                        _destructionTimer.resetAndStart();
                        this.GetComponent<WorldEntity>().TriggerConsumption(false);
                    }
                }
            }
        }
    }

    /**
     * Private
     */
    private Timer _destructionTimer;
    private Timer _throwScaleTimer;
    private bool _throwing;

    private const string THROW_VMOD = "THROW";
    
    private void throwCoin(LocalEventNotifier.Event e)
    {
        _throwing = true;
        this.Actor.enabled = true;
        VelocityModifier vMod = this.Actor.GetVelocityModifier(THROW_VMOD);
        vMod.Modifier = this.ThrowSpawnVelocity;
        vMod.Behavior = VelocityModifier.CollisionBehavior.bounce;
        vMod.Parameter = this.ThrowBounceMultiplier;
        this.Actor.SetVelocityModifier(THROW_VMOD, vMod);
    }

    private void onDestruction()
    {
        ObjectPools.Release(this.gameObject);
    }
}
