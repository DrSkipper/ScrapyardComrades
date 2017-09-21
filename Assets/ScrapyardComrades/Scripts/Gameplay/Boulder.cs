using UnityEngine;
using System.Collections.Generic;

public class Boulder : VoBehavior, IPausable
{
    public float Gravity = 10;
    public float MaxFallSpeed = 10;
    public float Friction = 7;
    public float AirFriction = 5;
    public float MinSpeedToDamage = 5;
    public float MinSpeedToDamageDown = 2;
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitParameters;
    public IntegerCollider DownDamageBox;
    public IntegerCollider UpDamageBox;
    public IntegerCollider LeftDamageBox;
    public IntegerCollider RightDamageBox;
    public PooledObject HitEffect;

    void Awake()
    {
        _nearbyDamagables = new List<IntegerCollider>();
        _damageRange = this.integerCollider.Bounds;
        _damageRange.Size += new IntegerVector(_damageRange.Size.X / 2, 0);
    }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        Vector2 velocity = this.Actor.Velocity;
        bool movingDown = velocity.y < -this.MinSpeedToDamageDown;
        bool movingUp = !movingDown && velocity.y >= this.MinSpeedToDamage;
        bool movingLeft = velocity.x <= -this.MinSpeedToDamage;
        bool movingRight = !movingLeft && velocity.x >= this.MinSpeedToDamage;

        GameObject groundedAgainst = this.GroundedAgainst;
        velocity.x = velocity.x.Approach(0.0f, groundedAgainst != null ? this.Friction : this.AirFriction);
        velocity.y = velocity.y.Approach(-this.MaxFallSpeed, this.Gravity);
        this.Actor.Velocity = velocity;

        if (movingDown || movingUp || movingLeft || movingRight)
        {
            _damageRange.Center = (IntegerVector)(Vector2)this.transform.position;
            CollisionManager.Instance.GetCollidersInRange(_damageRange, this.DamagableLayers, null, _nearbyDamagables);

            for (int i = _nearbyDamagables.Count - 1; i >= 0; --i)
            {
                IntegerCollider collider = _nearbyDamagables[i];
                if (collider == this.integerCollider)
                    _nearbyDamagables.RemoveAt(i);
            }

            if (movingDown)
                damage(this.DownDamageBox, 0, -1);
            if (movingUp)
                damage(this.UpDamageBox, 0, 1);
            if (movingLeft)
                damage(this.LeftDamageBox, -1, 0);
            if (movingRight)
                damage(this.RightDamageBox, 1, 0);
        }
    }

    public GameObject GroundedAgainst
    {
        get { return this.integerCollider.CollideFirst(0, -1, this.Actor.HaltMovementMask, null); }
    }

    /**
     * Private
     */
    private IntegerRect _damageRange;
    private List<IntegerCollider> _nearbyDamagables;

    private void damage(IntegerCollider collider, int targetDirX, int targetDirY)
    {
        GameObject collided = collider.CollideFirst(0, 0, this.DamagableLayers, null, _nearbyDamagables);
        if (collided != null)
        {
            IntegerVector colliderPos = new IntegerVector(Mathf.RoundToInt(collider.transform.position.x) + collider.Offset.X, Mathf.RoundToInt(collider.transform.position.y) + collider.Offset.Y);
            int dirX = Mathf.RoundToInt(Mathf.Sign(collided.transform.position.x - colliderPos.X));
            int dirY = Mathf.RoundToInt(Mathf.Sign(collided.transform.position.y - colliderPos.Y));
            if ((targetDirX == 0 || targetDirX == dirX) && (targetDirY == 0 || targetDirY == dirY))
            {
                Damagable damagable = collided.GetComponent<Damagable>();
                if (damagable != null)
                {
                    IntegerVector between = ((Vector2)collided.transform.position + (Vector2)colliderPos) / 2.0f; 
                    SCCharacterController.Facing facing = dirX == 1 ? SCCharacterController.Facing.Right : SCCharacterController.Facing.Left;
                    bool landedHit = damagable.Damage(this.HitParameters, (Vector2)this.transform.position, between, facing);

                    if (landedHit && this.HitParameters.HitAnimation != null)
                    {
                        AttackController.CreateHitEffect(this.HitEffect, this.HitParameters.HitAnimation, between, Damagable.FREEZE_FRAMES, facing);
                    }
                }
            }
        }
    }
}
