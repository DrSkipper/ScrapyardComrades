using UnityEngine;

public class SubwayTrain : VoBehavior, IPausable
{
    public Vector2 Velocity;
    public SimpleMovingPlatform[] CarPlatforms;
    public IntegerCollider[] Hurtboxes;
    public LayerMask DamagableLayerMask;
    public SCAttack.HitData HitData;
    public TrainFinishedDelegate OnReachedEndCallback;
    public SoundData.Key AppearSfxKey;
    public SoundData.Key RunningSfxKey;
    public int SfxInterval = 10;

    public delegate void TrainFinishedDelegate();

    public static bool TrainIsRunning { get; set; }

    void OnSpawn()
    {
        _sfxCycle = Random.Range(0, this.SfxInterval);
        if (_freezeFrameTimer == null)
            _freezeFrameTimer = new Timer(Damagable.FREEZE_FRAMES, false, false);
        _freezeFrameTimer.complete();

        for (int i = 0; i < this.CarPlatforms.Length; ++i)
        {
            this.CarPlatforms[i].integerCollider.AddToCollisionPool();
            this.CarPlatforms[i].Velocity = this.Velocity;
        }

        TrainIsRunning = true;
        SoundManager.Play(this.AppearSfxKey, this.transform);
    }

    void FixedUpdate()
    {
        if (_sfxCycle < this.SfxInterval)
            ++_sfxCycle;

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
                        // Move the hit object manually up a bit just in case
                        if (damagable.Actor != null)
                            damagable.Actor.Move(new IntegerVector(0, 4));

                        damagable.Damage(this.HitData, (Vector2)this.transform.position, (Vector2)this.transform.position, SCCharacterController.Facing.Right);
                        _freezeFrameTimer.reset();
                        _freezeFrameTimer.start();
                    }
                }
            }
            
            if (this.transform.position.x > TRAIN_END_POSITION)
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

        if (_sfxCycle >= this.SfxInterval)
        {
            _sfxCycle = 0;
            SoundManager.Play(this.RunningSfxKey, this.transform);
        }
    }

    void OnReturnToPool()
    {
        for (int i = 0; i < this.CarPlatforms.Length; ++i)
        {
            this.CarPlatforms[i].integerCollider.RemoveFromCollisionPool();
        }

        TrainIsRunning = false;
    }

    /**
     * Private
     */
    private Timer _freezeFrameTimer;
    private int _sfxCycle;

    private const int TRAIN_END_POSITION = 2220;
}
