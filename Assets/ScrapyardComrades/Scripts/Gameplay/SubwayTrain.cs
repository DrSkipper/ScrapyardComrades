using UnityEngine;

public class SubwayTrain : VoBehavior, IPausable
{
    public Vector2 Velocity;
    public SimpleMovingPlatform[] CarPlatforms;
    public IntegerCollider[] Hurtboxes;
    public LayerMask DamagableLayerMask;
    public SCAttack.HitData HitData;
    public TrainFinishedDelegate OnReachedEndCallback;

    public delegate void TrainFinishedDelegate();

    void OnSpawn()
    {
        if (_freezeFrameTimer == null)
            _freezeFrameTimer = new Timer(Damagable.FREEZE_FRAMES, false, false);
        _freezeFrameTimer.complete();

        for (int i = 0; i < this.CarPlatforms.Length; ++i)
        {
            this.CarPlatforms[i].integerCollider.AddToCollisionPool();
            this.CarPlatforms[i].Velocity = this.Velocity;
        }
    }

    void FixedUpdate()
    {
        if (_freezeFrameTimer.Completed)
        {
            this.transform.SetPosition2D(this.transform.position.x + this.Velocity.x, this.transform.position.y + this.Velocity.y);

            for (int i = 0; i < this.Hurtboxes.Length; ++i)
            {
                GameObject collided = this.Hurtboxes[i].CollideFirst(0, 0, this.DamagableLayerMask);

                if (collided != null)
                {
                    Damagable damagable = collided.GetComponent<Damagable>();
                    if (damagable != null)
                    {
                        damagable.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, SCCharacterController.Facing.Right);
                        _freezeFrameTimer.reset();
                        _freezeFrameTimer.start();
                    }
                }
            }

            //TODO: Figure out best way to check when train has reached end
            if (this.transform.position.x > 2500)
            {
                if (this.OnReachedEndCallback != null)
                    this.OnReachedEndCallback();
                ObjectPools.Release(this.gameObject);
            }
        }
        else
        {
            _freezeFrameTimer.update();
        }
    }

    void OnReturnToPool()
    {
        for (int i = 0; i < this.CarPlatforms.Length; ++i)
        {
            this.CarPlatforms[i].integerCollider.RemoveFromCollisionPool();
        }
    }

    /**
     * Private
     */
    private Timer _freezeFrameTimer;
}
