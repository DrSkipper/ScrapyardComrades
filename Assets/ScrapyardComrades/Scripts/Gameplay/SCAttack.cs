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

    public SCSpriteAnimation SpriteAnimation;
    public int NormalFrameLength;
    public HitboxKeyframe[] HitboxKeyframes;
    public int JumpInterruptFrame;
    public int LedgeGrabInterruptFrame;
    public int DodgeInterruptFrame;
    public int ThrowInterruptFrame;
    public int AttackInterruptFrame; // Have this be different by attack type?
    public int AttackCooldown; // Should also probably be by attack type

    public float KnockbackPower = 10.0f;
    public int HitInvincibilityDuration = 4;
    public int HitStunDuration = 4;
    //TODO - 4 types of knockback - Hard-set direction, 1stChar-to-2ndChar origin difference, 1stChar-to-hit, hit-to-2ndChar

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
