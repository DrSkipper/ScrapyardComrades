using UnityEngine;

public class DamageOnCollision : VoBehavior
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;
    public float MinVelocityToDamage = 3.0f;
    public bool AlwaysDestroyOnCollide = false;
    public PooledObject DestructionPrefab; //TODO
    public SCSpriteAnimation DestructionAnimation;
    public PooledObject HitEffectPrefab;
    public SCSpriteAnimation HitEffectAnimation;

    void Awake()
    {
        this.localNotifier.Listen(CollisionEvent.NAME, this, onCollide);
    }

    public void ConfigureForPickup(SCPickup pickup)
    {
        this.HitData.Damage = pickup.Damage;
        this.HitData.HitStunDuration = pickup.StunTime;
    }

    void FixedUpdate()
    {
        _prevPos = this.integerPosition;
    }

    private IntegerVector _prevPos;

    private void onCollide(LocalEventNotifier.Event e)
    {
        Vector2 totalV = this.Actor.TotalVelocity;

        if (totalV.magnitude >= this.MinVelocityToDamage)
        {
            CollisionEvent ce = e as CollisionEvent;
            for (int i = 0; i < ce.Hits.Count; ++i)
            {
                LayerMask collisionMask = 1 << ce.Hits[i].layer;
                if ((collisionMask & this.DamagableLayers) == collisionMask)
                {
                    ce.Hits[i].GetComponent<Damagable>().Damage(
                        this.HitData,
                        _prevPos,
                        this.integerPosition,
                        this.Actor.TotalVelocity.x > 0 ?
                        SCCharacterController.Facing.Right :
                        SCCharacterController.Facing.Left);
                }
            }
        }

        if (this.AlwaysDestroyOnCollide)
            ObjectPools.Release(this.gameObject);
    }
}
