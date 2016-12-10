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
    public SCAttack GetAttackForInput(SCCharacterController.InputWrapper input, SCCharacterController character, int categoryConstraints = int.MaxValue)
    {
        if (character.OnGround)
        {
            if (input.DodgeBegin && validMove(this.GroundDodge, categoryConstraints))
                return this.GroundDodge;
            if (input.AttackLightBegin && validMove(this.GroundNeutral, categoryConstraints))
                return this.GroundNeutral;
        }
        else
        {
            if (input.DodgeBegin && validMove(this.AirDodge, categoryConstraints))
                return this.AirDodge;
            if (input.AttackLightBegin && validMove(this.AirNeutral, categoryConstraints))
                return this.AirNeutral;
        }
        return null;
    }

    private static bool validMove(SCAttack move, int categoryConstraints)
    {
        return move != null && ((int)move.Category & categoryConstraints) != 0;
    }
}
