using UnityEngine;

public class SCAttack : ScriptableObject
{
    // Attack type enum?
    // Also probably need some sort of "sub attack"/"sub animation" set up so there can be multiple animations and/or stages associated with an attack

    public SCSpriteAnimation SpriteAnimation;
    public int NormalFrameLength;
    public int JumpInterruptFrame;
    public int LedgeGrabInterruptFrame;
    public int DodgeInterruptFrame;
    public int ThrowInterruptFrame;
    public int AttackInterruptFrame; // Have this be different by attack type?
}
