using UnityEngine;

public class AttackController : VoBehavior, IPausable
{
    public IntegerRectCollider[] DamageBoxes;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimator EffectAnimator;
    public Damagable Damagable;
    public LayerMask DamagableLayers;
    public PooledObject HitEffect;
    public HurtboxChangeDelegate HurtboxChangeCallback;
    public delegate bool HurtboxChangeDelegate(SCAttack.HurtboxState newState);

    public void AddDamageBoxes()
    {
        for (int i = 0; i < this.DamageBoxes.Length; ++i)
        {
            this.DamageBoxes[i].AddToCollisionPool();
        }
    }

    void OnReturnToPool()
    {
        for (int i = 0; i < this.DamageBoxes.Length; ++i)
        {
            this.DamageBoxes[i].RemoveFromCollisionPool();
        }
    }

    public void UpdateHitBoxes(SCAttack currentAttack, SCAttack.HurtboxState hurtboxState, SCCharacterController.Facing facing)
    {
        if (currentAttack == null)
        {
            for (int i = 0; i < this.DamageBoxes.Length; ++i)
            {
                this.DamageBoxes[i].transform.localPosition = Vector2.zero;
                this.DamageBoxes[i].enabled = false;
            }
            this.EffectAnimator.gameObject.SetActive(false);
        }
        else
        {
            // Activate effect if necessary
            if (this.EffectAnimator != null)
            {
                SCAttack.Effect? effect = getEffectForUpdateFrame(currentAttack, this.Animator.Elapsed);
                if (effect.HasValue)
                {
                    this.EffectAnimator.gameObject.SetActive(true);
                    this.EffectAnimator.transform.SetLocalPosition2D(effect.Value.Position.X, effect.Value.Position.Y);
                    this.EffectAnimator.PlayAnimation(effect.Value.Animation, false);
                }
                else if (!this.EffectAnimator.IsPlaying)
                {
                    this.EffectAnimator.gameObject.SetActive(false);
                }
            }

            // Update hitboxes
            SCAttack.HitboxKeyframe? keyframe = getKeyframeForUpdateFrame(currentAttack, this.Animator.Elapsed);
            if (keyframe.HasValue)
            {
                if (keyframe.Value.HurtboxState != hurtboxState)
                {
                    bool success = this.HurtboxChangeCallback(keyframe.Value.HurtboxState);
                    if (!success)
                        return;
                }

                GameObject collided = null;
                IntegerRectCollider collider = null;
                for (int i = 0; i < this.DamageBoxes.Length; ++i)
                {
                    if (i < keyframe.Value.HitboxCount)
                    {
                        this.DamageBoxes[i].enabled = true;
                        this.DamageBoxes[i].transform.localPosition = (Vector2)keyframe.Value.HitboxPositions[i];
                        this.DamageBoxes[i].Size = keyframe.Value.HitboxSizes[i];

                        if (collided == null)
                        {
                            collided = this.DamageBoxes[i].CollideFirst(0, 0, this.DamagableLayers);
                            if (collided != null)
                                collider = this.DamageBoxes[i];
                        }
                    }
                    else
                    {
                        this.DamageBoxes[i].enabled = false;
                    }
                }

                // Apply damage if we hit
                if (collided != null)
                {
                    Damagable otherDamagable = collided.GetComponent<Damagable>();
                    if (otherDamagable != null)
                    {
                        IntegerVector hitPoint = collided.GetComponent<IntegerCollider>().ClosestContainedPoint((Vector2)collider.transform.position);
                        bool landedHit = otherDamagable.Damage(currentAttack.HitParameters, (Vector2)this.Actor.transform.position, hitPoint, facing);

                        if (landedHit)
                        {
                            if (_freezeFrameEvent == null)
                                _freezeFrameEvent = new FreezeFrameEvent(Damagable.FREEZE_FRAMES);
                            this.localNotifier.SendEvent(_freezeFrameEvent);
                            if (this.Damagable != null)
                                this.Damagable.SetInvincible(Damagable.FREEZE_FRAMES);
                            PooledObject hitEffect = this.HitEffect.Retain();
                            hitEffect.transform.position = (Vector2)hitPoint;
                            hitEffect.GetComponent<HitEffectHandler>().InitializeWithFreezeFrames(Damagable.FREEZE_FRAMES);
                        }
                    }
                }
            }
        }
    }

    public SCAttack.VelocityBoost? GetCurrentVelocityBoost(SCAttack currentAttack)
    {
        return currentAttack.GetVelocityBoostForFrame(this.Animator.Elapsed);
    }

    /**
     * Private
     */
    private FreezeFrameEvent _freezeFrameEvent;

    private SCAttack.HitboxKeyframe? getKeyframeForUpdateFrame(SCAttack attack, int updateFrame)
    {
        SCAttack.HitboxKeyframe? keyframe = null;
        for (int i = 0; i < attack.HitboxKeyframes.Length; ++i)
        {
            if (attack.HitboxKeyframes[i].Frame <= updateFrame)
                keyframe = attack.HitboxKeyframes[i];
            else
                break;
        }
        return keyframe;
    }

    private SCAttack.Effect? getEffectForUpdateFrame(SCAttack move, int updateFrame)
    {
        if (move.Effects != null)
        {
            for (int i = 0; i < move.Effects.Length; ++i)
            {
                if (move.Effects[i].Frame == updateFrame)
                    return move.Effects[i];
            }
        }
        return null;
    }
}
