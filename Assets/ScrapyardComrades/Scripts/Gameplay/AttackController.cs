using UnityEngine;

public class AttackController : VoBehavior
{
    public IntegerRectCollider[] DamageBoxes;
    public IntegerRectCollider Hurtbox;
    public SCSpriteAnimator Animator;
    public Damagable Damagable;
    public LayerMask DamagableLayers;
    public GameObject HitEffect;

    /*void OnStart()
    {
        this.SetDefaults();
    }

    public void SetDefaults()
    {
        this.DefaultHurtboxOffset = this.Hurtbox.Offset;
        this.DefaultHurtboxSize = this.Hurtbox.Size;
    }*/

    public void UpdateHitBoxes(SCAttack currentAttack, LayerMask haltMovementMask)
    {
        if (currentAttack == null)
        {
            for (int i = 0; i < this.DamageBoxes.Length; ++i)
            {
                this.DamageBoxes[i].transform.localPosition = Vector2.zero;
                this.DamageBoxes[i].enabled = false;
            }
            this.Hurtbox.enabled = true;
            //attemptHitboxChange(this.DefaultHurtboxOffset, this.DefaultHurtboxSize, haltMovementMask);
        }
        else
        {
            SCAttack.HitboxKeyframe? keyframe = getKeyframeForUpdateFrame(currentAttack, this.Animator.Elapsed);
            if (keyframe.HasValue)
            {
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
                if (keyframe.Value.HurtboxRect.Size != IntegerVector.Zero)
                {
                    attemptHitboxChange(keyframe.Value.HurtboxRect.Center, keyframe.Value.HurtboxRect.Size, haltMovementMask);
                }
                else
                {
                    this.Hurtbox.enabled = false;
                }

                // Apply damage if we hit
                if (collided != null)
                {
                    Damagable otherDamagable = collided.GetComponent<Damagable>();
                    if (otherDamagable != null)
                    {
                        IntegerVector hitPoint = collided.GetComponent<IntegerCollider>().ClosestContainedPoint((Vector2)collider.transform.position);
                        bool landedHit = otherDamagable.Damage(currentAttack, (Vector2)this.Actor.transform.position, hitPoint);

                        if (landedHit)
                        {
                            this.localNotifier.SendEvent(new FreezeFrameEvent(Damagable.FreezeFrames));
                            if (this.Damagable != null)
                                this.Damagable.SetInvincible(Damagable.FreezeFrames);
                            GameObject hitEffect = Instantiate<GameObject>(this.HitEffect);
                            hitEffect.transform.position = (Vector2)hitPoint;
                            hitEffect.GetComponent<HitEffectHandler>().InitializeWithFreezeFrames(Damagable.FreezeFrames);
                        }
                    }
                }
            }
        }
    }

    /**
     * Private
     */
    private IntegerVector DefaultHurtboxOffset;
    private IntegerVector DefaultHurtboxSize;

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

    private void attemptHitboxChange(IntegerVector offset, IntegerVector size, LayerMask haltMovementMask)
    {
        if (!this.Hurtbox.enabled || offset != this.Hurtbox.Offset || size != this.Hurtbox.Size)
        {
            this.Hurtbox.enabled = true;
            this.Hurtbox.Offset = offset;
            this.Hurtbox.Size = size;
            if (this.Hurtbox.CollideFirst(0, 0, haltMovementMask) != null)
            {
                //this.Animator.Loop()
                this.localNotifier.SendEvent(new CharacterForceDuckEvent());
            }
        }
    }
}
