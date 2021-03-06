﻿using UnityEngine;

public class SCAttack : ScriptableObject
{
    // Also probably need some sort of "sub attack"/"sub animation" set up so there can be multiple animations and/or stages associated with an attack

    [System.Serializable]
    public enum MoveCategory
    {
        None = 0,
        Normal = 2,
        Dodge = 4,
        Combo = 8
        //Recovery?
    }

    [System.Serializable]
    public struct HitboxKeyframe
    {
        public int Frame;
        public int VisualFrame;
        public int HitboxCount;
        public IntegerVector[] HitboxPositions;
        public IntegerVector[] HitboxSizes;
        public HurtboxState HurtboxState;
    }

    [System.Serializable]
    public enum HurtboxState
    {
        Normal,
        Ducking
    }

    [System.Serializable]
    public struct VelocityBoost
    {
        public int EffectFrame;
        public Vector2 Boost;
        public BoostType Type;
        public int DurationFrames;

        [System.Serializable]
        public enum BoostType
        {
            None,
            Additive,
            Average,
            Absolute,
            AverageXOnly
        }
    }

    [System.Serializable]
    public struct HitData
    {
        public int Damage;
        public KnockbackType KnockbackType;
        public VectorExtensions.Direction16 KnockbackDirection;
        public float KnockbackPower;
        public int HitInvincibilityDuration;
        public int HitStunDuration;
        public float HitStunGravityMultiplier;
        public float HitStunAirFrictionMultiplier;
        public SoundData.Key HitSfxKey;
        public SCSpriteAnimation HitAnimation;
        public int Level;

        public Vector2 GetKnockback(Vector2 attackerPos, Vector2 defenderPos, Vector2 hitPos, SCCharacterController.Facing attackerFacing, bool cardinalOnly)
        {
            switch (this.KnockbackType)
            {
                default:
                case KnockbackType.None:
                    return Vector2.zero;
                case KnockbackType.Constant:
                    Vector2 knockback = VectorExtensions.VectorFromDirection16(this.KnockbackDirection);
                    if (cardinalOnly)
                        knockback = knockback.NormalizedCardinal();
                    knockback *= this.KnockbackPower;
                    if (attackerFacing == SCCharacterController.Facing.Left)
                        knockback = new Vector2(knockback.x * -1, knockback.y);
                    return knockback;
                case KnockbackType.CharacterDiff:
                    return (!cardinalOnly ? 
                        (defenderPos - attackerPos).Normalized16() : 
                        (defenderPos - attackerPos).NormalizedCardinal())
                        * this.KnockbackPower;
                case KnockbackType.AttackerToHitPoint:
                    return (!cardinalOnly ? 
                        (hitPos - attackerPos).Normalized16() :
                        (hitPos - attackerPos).NormalizedCardinal())
                        * this.KnockbackPower;
                case KnockbackType.HitPointToDefender:
                    return (!cardinalOnly ? 
                        (defenderPos - hitPos).Normalized16() :
                        (defenderPos - hitPos).NormalizedCardinal())
                        * this.KnockbackPower;
                case KnockbackType.AvgConstantAndDiff:
                    Vector2 constant = VectorExtensions.VectorFromDirection16(this.KnockbackDirection);
                    if (attackerFacing == SCCharacterController.Facing.Left)
                        constant = new Vector2(constant.x * -1, constant.y);
                    Vector2 charDiff = (defenderPos - attackerPos).normalized;
                    return (!cardinalOnly ?
                        ((constant + charDiff) / 2.0f).Normalized16() :
                        ((constant + charDiff) / 2.0f).NormalizedCardinal())
                        * this.KnockbackPower;
                case KnockbackType.ReverseCharacterDiff:
                    return (!cardinalOnly ?
                        (attackerPos - defenderPos).Normalized16() :
                        (attackerPos - defenderPos).NormalizedCardinal())
                        * this.KnockbackPower;
            }
        }
    }

    [System.Serializable]
    public enum KnockbackType
    {
        None,
        Constant,
        CharacterDiff,
        AttackerToHitPoint,
        HitPointToDefender,
        AvgConstantAndDiff,
        ReverseCharacterDiff
    }

    [System.Serializable]
    public struct Combo
    {
        public SCAttack ComboMove;
        public SCMoveSet.MoveInput Input;
    }

    [System.Serializable]
    public struct Effect
    {
        public SCSpriteAnimation Animation;
        public int Frame;
        public IntegerVector Position;
    }

    [System.Serializable]
    public struct ThrowFrame
    {
        public int Frame;
        public IntegerVector OriginOffset;
        public Vector2 ThrowDirection;
        public float ThrowVelocity;
        public bool RotateSprite;
    }

    public MoveCategory Category;
    public SCSpriteAnimation SpriteAnimation;
    public int NormalFrameLength;
    public HitboxKeyframe[] HitboxKeyframes;
    public int JumpInterruptFrame;
    public int LedgeGrabInterruptFrame;
    public MoveCategory[] MoveInterruptCategories;
    public int MoveInterruptFrame;
    public MoveCategory[] CooldownCategories;
    public int CooldownDuration;
    public int ComboWindow;
    public int ComboBuffer;

    [System.Serializable]
    public enum OnGroundEffect
    {
        None,
        Stop,
        Combo
    }

    public OnGroundEffect GroundedEffect;
    public SCAttack GroundedCombo;
    public int GroundedFramesForEffect;
    public Combo[] Combos;
    public HitData HitParameters;
    public Effect[] Effects;
    public IntegerVector[] BlockActiveRanges;
    public ThrowFrame[] ThrowFrames;
    public PooledObject PrefabToThrow;

    public float GravityMultiplier = 1.0f;
    public float JumpPowerMultiplier = 1.0f;
    public float JumpHorizontalBoostMultiplier = 1.0f;
    public float FrictionMultiplier = 1.0f;
    public float AirFrictionMultiplier = 1.0f;
    public float MaxRunSpeedMultiplier = 1.0f;
    public float RunAccelerationMultiplier = 1.0f;
    public float RunDeccelerationMultiplier = 1.0f;
    public float AirRunAccelerationMultiplier = 1.0f;

    public VelocityBoost[] VelocityBoosts;
    public bool LockInput = false;
    public bool LockMovement = false;

    public VelocityBoost? GetVelocityBoostForFrame(int frame)
    {
        if (this.VelocityBoosts == null)
            return null;
        for (int i = 0; i < this.VelocityBoosts.Length; ++i)
        {
            if (this.VelocityBoosts[i].EffectFrame <= frame && this.VelocityBoosts[i].EffectFrame + this.VelocityBoosts[i].DurationFrames > frame)
                return this.VelocityBoosts[i];
        }
        return null;
    }

    public int MoveInterruptCategoryMask { get { return categoryArrayToMask(this.MoveInterruptCategories); } }
    public int CooldownCategoriesMask { get { return categoryArrayToMask(this.CooldownCategories); } }

    /**
     * Private
     */
    private static int categoryArrayToMask(MoveCategory[] array)
    {
        int mask = 0;
        for (int i = 0; i < array.Length; ++i)
        {
            mask |= (int)array[i];
        }
        return mask;
    }
}
