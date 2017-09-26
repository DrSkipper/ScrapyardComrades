using UnityEngine;
using System.Collections.Generic;

public class AttackController : VoBehavior, IPausable
{
    public IntegerRectCollider[] DamageBoxes;
    public IntegerCollider Hurtbox;
    public SCSpriteAnimator Animator;
    public SCSpriteAnimator EffectAnimator;
    public Damagable Damagable;
    public LayerMask DamagableLayers;
    public LayerMask BlockableLayers;
    public PooledObject HitEffect;
    public SCSpriteAnimation BlockEffectAnim;
    public HurtboxChangeDelegate HurtboxChangeCallback;
    public delegate bool HurtboxChangeDelegate(SCAttack.HurtboxState newState);

    void Awake()
    {
        _potentialCollisions = new List<IntegerCollider>();
    }

    public void UpdateHitBoxes(SCAttack currentAttack, SCAttack.HurtboxState hurtboxState, SCCharacterController.Facing facing)
    {
        if (currentAttack == null)
        {
            if (_attacking)
                clear();
        }
        else
        {
            _attacking = true;
            activateEffectIfNecessary(currentAttack);

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
                bool blocked = false;
                for (int i = 0; i < this.DamageBoxes.Length; ++i)
                {
                    if (i < keyframe.Value.HitboxCount)
                    {
                        IntegerRectCollider box = this.DamageBoxes[i];
                        box.enabled = true;
                        box.transform.localPosition = (Vector2)keyframe.Value.HitboxPositions[i];
                        box.Size = keyframe.Value.HitboxSizes[i];

                        if (collided == null)
                        {
                            // Check if we hit a damagable with this hitbox
                            box.GetPotentialCollisions(0, 0, 0, 0, this.DamagableLayers, _potentialCollisions);
                            if (this.DamagableLayers.ContainsLayer(this.gameObject.layer))
                                _potentialCollisions.Remove(this.Hurtbox);
                            collided = box.CollideFirst(0, 0, this.DamagableLayers, null, _potentialCollisions);

                            if (collided != null)
                            {
                                collider = box;

                                // Check if the attack was blocked
                                CollisionManager.RaycastResult result = this.CollisionManager.Raycast(this.integerPosition, this.transform.DirectionTo2D(collided.transform), this.transform.Distance2D(collided.transform), (this.BlockableLayers | this.DamagableLayers));
                                if (result.Collided)
                                {
                                    GameObject potentialBlocker = null;
                                    for (int j = 0; j < result.Collisions.Length; ++j)
                                    {
                                        if (result.Collisions[j].CollidedObject != this.gameObject)
                                        {
                                            potentialBlocker = result.Collisions[j].CollidedObject;
                                            break;
                                        }
                                    }
                                    if (potentialBlocker != null && this.BlockableLayers.ContainsLayer(potentialBlocker.layer))
                                    {
                                        collided = potentialBlocker;
                                        blocked = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Make sure to disable unused hitboxes
                        this.DamageBoxes[i].enabled = false;
                    }
                }
                
                // If blocked, freeze-frame then cancel attack
                if (blocked)
                {
                    notifyFreezeFrames(Damagable.FREEZE_FRAMES);
                    notifyOtherBlocker(collided, Damagable.FREEZE_FRAMES, currentAttack.HitParameters);

                    IntegerVector hitPoint = collided.GetComponent<IntegerCollider>().ClosestContainedPoint((Vector2)collider.transform.position);
                    CreateHitEffect(this.HitEffect, this.BlockEffectAnim, hitPoint, Damagable.FREEZE_FRAMES, facing);

                    if (_cancelAttackEvent == null)
                        _cancelAttackEvent = new CancelAttackEvent();
                    this.localNotifier.SendEvent(_cancelAttackEvent);
                }

                // Apply damage if we hit
                else if (collided != null)
                {
                    Damagable otherDamagable = collided.GetComponent<Damagable>();
                    if (otherDamagable != null)
                    {
                        IntegerVector hitPoint = collided.GetComponent<IntegerCollider>().ClosestContainedPoint((Vector2)collider.transform.position);
                        bool landedHit = otherDamagable.Damage(currentAttack.HitParameters, (Vector2)this.Actor.transform.position, hitPoint, facing);

                        if (landedHit)
                        {
                            int freezeFrames = otherDamagable.Dead ? Damagable.DEATH_FREEZE_FRAMES : Damagable.FREEZE_FRAMES;

                            notifyFreezeFrames(freezeFrames);
                            CreateHitEffect(this.HitEffect, currentAttack.HitParameters.HitAnimation, hitPoint, freezeFrames, facing);

                            if (this.EffectAnimator != null && this.EffectAnimator.IsPlaying)
                                this.EffectAnimator.freezeFrame(null);
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

    public bool CanBlock(SCAttack currentAttack)
    {
        if (currentAttack == null)
            return true;
        if (currentAttack.BlockActiveRanges != null && currentAttack.BlockActiveRanges.Length > 0)
        {
            int elapsed = this.Animator.Elapsed;
            for (int i = 0; i < currentAttack.BlockActiveRanges.Length; ++i)
            {
                IntegerVector range = currentAttack.BlockActiveRanges[i];
                if (elapsed >= range.X && elapsed <= range.Y)
                    return true;
            }
        }
        return false;
    }
    
    public static void CreateHitEffect(PooledObject hitEffectPrefab, SCSpriteAnimation anim, Vector2 hitPoint, int freezeFrames, SCCharacterController.Facing facing)
    {
        PooledObject hitEffect = hitEffectPrefab.Retain();
        hitEffect.transform.SetPosition2D(hitPoint);
        hitEffect.GetComponent<HitEffectHandler>().InitializeWithFreezeFrames(freezeFrames, anim, (int)facing);
    }

    /**
     * Private
     */
    private FreezeFrameEvent _freezeFrameEvent;
    private CancelAttackEvent _cancelAttackEvent;
    private bool _attacking;
    private List<IntegerCollider> _potentialCollisions;
    
    //private const int FRAMES_BETWEEN_COLLIDER_GET = 4;

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
    
    private void notifyFreezeFrames(int freezeFrames)
    {
        if (_freezeFrameEvent == null)
            _freezeFrameEvent = new FreezeFrameEvent(freezeFrames);
        else
            _freezeFrameEvent.NumFrames = freezeFrames;

        this.localNotifier.SendEvent(_freezeFrameEvent);

        if (this.Damagable != null)
            this.Damagable.SetInvincible(freezeFrames);
    }

    private void notifyOtherBlocker(GameObject collided, int freezeFrames, SCAttack.HitData hitData)
    {
        BlockHandler blocker = collided.GetComponent<BlockHandler>();
        if (blocker != null)
            blocker.HandleBlock(freezeFrames, hitData);
    }

    private void activateEffectIfNecessary(SCAttack currentAttack)
    {
        if (this.EffectAnimator != null)
        {
            // Make sure effect animator isn't staying frozen
            this.EffectAnimator.freezeFrameEnded(null);

            // Activate effect if necessary
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
    }

    private void clear()
    {
        _attacking = false;
        for (int i = 0; i < this.DamageBoxes.Length; ++i)
        {
            this.DamageBoxes[i].transform.localPosition = Vector2.zero;
            this.DamageBoxes[i].enabled = false;
        }

        if (this.EffectAnimator != null)
            this.EffectAnimator.gameObject.SetActive(false);
    }
}

public class CancelAttackEvent : LocalEventNotifier.Event
{
    public const string NAME = "ATK_CANCEL";

    public CancelAttackEvent()
    {
        this.Name = NAME;
    }
}
