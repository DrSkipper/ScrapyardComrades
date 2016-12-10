using UnityEngine;
using System.Collections.Generic;

public class SCMoveSet : ScriptableObject
{
    public IntegerRect NormalHitboxSpecs;
    public IntegerRect DuckHitboxSpecs;
    public SCAttack GroundNeutral;
    public SCAttack GroundDodge;
    public SCAttack AirNeutral;
    public SCAttack AirDodge;

    //TODO: Expand as more attack options, combos, etc are added
    // Also need to update SCMoveSetEditingEditor when more moves are added
    public SCAttack GetAttackForInput(SCCharacterController.InputWrapper input, SCCharacterController character)
    {
        if (character.OnGround)
        {
            if (input.DodgeBegin)
                return this.GroundDodge;
            if (input.AttackLightBegin)
                return this.GroundNeutral;
        }
        else
        {
            if (input.DodgeBegin)
                return this.AirDodge;
            if (input.AttackLightBegin)
                return this.AirNeutral;
        }
        return null;
    }
}
