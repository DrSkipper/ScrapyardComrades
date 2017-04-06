using UnityEngine;

public class SCAttack : ScriptableObject
{
    // Also probably need some sort of "sub attack"/"sub animation" set up so there can be multiple animations and/or stages associated with an attack

    [System.Serializable]
    public enum MoveCategory
    {
        None = 0,
        Normal = 2,
        Dodge = 4
        //Combo = 8
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

    [System.Serializable]
    public struct Combo
    {
        public SCAttack ComboMove;
        public SCMoveSet.MoveInput Input;
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

    public Combo[] Combos;
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
