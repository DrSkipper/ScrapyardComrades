using UnityEngine;

public class SCAttack : ScriptableObject
{
    // Attack type enum?
    // Also probably need some sort of "sub attack"/"sub animation" set up so there can be multiple animations and/or stages associated with an attack

    public SCSpriteAnimation SpriteAnimation;
    public int NormalFrameLength;
    public IntegerVector[] HitFrameRanges;
    public int JumpInterruptFrame;
    public int LedgeGrabInterruptFrame;
    public int DodgeInterruptFrame;
    public int ThrowInterruptFrame;
    public int AttackInterruptFrame; // Have this be different by attack type?
    public int AttackCooldown; // Should also probably be by attack type

    public float GravityMultiplier = 1.0f;
    public float JumpPowerMultiplier = 1.0f;
    public float JumpHorizontalBoostMultiplier = 1.0f;
    public float FrictionMultiplier = 1.0f;
    public float AirFrictionMultiplier = 1.0f;
    public float MaxRunSpeedMultiplier = 1.0f;
    public float RunAccelerationMultiplier = 1.0f;
    public float RunDeccelerationMultiplier = 1.0f;
    public float AirRunAccelerationMultiplier = 1.0f;

    public PlayerController.VelocityBoost[] VelocityBoosts;
}
