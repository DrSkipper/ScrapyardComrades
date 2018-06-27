using UnityEngine;

public class DamageOnCollision : VoBehavior
{
    public LayerMask DamagableLayers;
    public SCAttack.HitData HitData;
    public float MinVelocityToDamage = 3.0f;
    public bool AlwaysDestroyOnCollide = false;
    public PooledObject HitEffectPrefab;
    public SCSpriteAnimation DestructionAnimation;
    public SoundData.Key DestructionSoundKey;

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
        bool hitTarget = false;

        if (totalV.magnitude >= this.MinVelocityToDamage)
        {
            CollisionEvent ce = e as CollisionEvent;
            for (int i = 0; i < ce.Hits.Count; ++i)
            {
                GameObject hit = ce.Hits[i];
                LayerMask collisionMask = 1 << hit.layer;
                if ((collisionMask & this.DamagableLayers) == collisionMask)
                {
                    hitTarget = true;
                    ce.Hits[i].GetComponent<Damagable>().Damage(
                        this.HitData,
                        _prevPos,
                        this.integerPosition,
                        this.Actor.TotalVelocity.x > 0 ?
                        SCCharacterController.Facing.Right :
                        SCCharacterController.Facing.Left);

                    if (this.HitEffectPrefab != null)
                    {
                        PooledObject hitEffect = this.HitEffectPrefab.Retain();
                        hitEffect.transform.SetPosition2D((hit.transform.position + this.transform.position) / 2);
                        hitEffect.GetComponent<HitEffectHandler>().InitializeWithFreezeFrames(Damagable.FREEZE_FRAMES, this.HitData.HitAnimation, Mathf.RoundToInt(Mathf.Sign(hit.transform.position.x - this.transform.position.x)));
                    }
                }
            }
        }

        if (this.AlwaysDestroyOnCollide)
        {
            Transform soundTransform = this.transform;

            if (this.DestructionAnimation != null)
            {
                PooledObject effect = this.HitEffectPrefab.Retain();
                effect.transform.SetPosition2D(this.transform.position);
                effect.GetComponent<HitEffectHandler>().InitializeWithFreezeFrames(hitTarget ? Damagable.FREEZE_FRAMES : 0, this.DestructionAnimation, 1);
                soundTransform = effect.transform;
            }

            SoundManager.Play(this.DestructionSoundKey, this.transform);
            ObjectPools.Release(this.gameObject);
        }
    }
}
