using UnityEngine;

public class SCMoveSet : ScriptableObject
{
    public SCAttack GroundNeutral;
    public SCAttack Dodge;

    //TODO: Expand as more attack options, combos, etc are added
    public SCAttack GetAttackForInput(SCCharacterController.InputWrapper input)
    {
        if (input.DodgeBegin)
            return this.Dodge;
        if (input.AttackLightBegin)
            return this.GroundNeutral;
        return null;
    }
}
