using UnityEngine;

public class SCAttack : ScriptableObject
{
    // Attack type enum?
    // Also probably need some sort of "sub attack"/"sub animation" set up so there can be multiple animations and/or stages associated with an attack

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
            Absolute
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

        public Vector2 GetKnockback(Vector2 attackerPos, Vector2 defenderPos, Vector2 hitPos, SCCharacterController.Facing attackerFacing)
        {
            switch (this.KnockbackType)
            {
                default:
                case KnockbackType.None:
                    return Vector2.zero;
                case KnockbackType.Constant:
                    Vector2 knockback = VectorExtensions.VectorFromDirection16(this.KnockbackDirection) * this.KnockbackPower;
                    if ((int)attackerFacing == -1)
                        knockback = new Vector2(knockback.x * -1, knockback.y);
                    return knockback;
                case KnockbackType.CharacterDiff:
                    return (defenderPos - attackerPos).Normalized16() * this.KnockbackPower;
                case KnockbackType.AttackerToHitPoint:
                    return (hitPos - attackerPos).Normalized16() * this.KnockbackPower;
                case KnockbackType.HitPointToDefender:
                    return (defenderPos - hitPos).Normalized16() * this.KnockbackPower;
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
        HitPointToDefender
    }

    public SCSpriteAnimation SpriteAnimation;
    public int NormalFrameLength;
    public HitboxKeyframe[] HitboxKeyframes;
    public int JumpInterruptFrame;
    public int LedgeGrabInterruptFrame;
    public int DodgeInterruptFrame;
    public int ThrowInterruptFrame;
    public int AttackInterruptFrame; // Have this be different by attack type?
    public int AttackCooldown; // Should also probably be by attack type

    public HitData HitParameters;

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
}
