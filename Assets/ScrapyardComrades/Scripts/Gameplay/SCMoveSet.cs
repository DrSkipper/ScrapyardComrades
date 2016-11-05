using UnityEngine;
using System.Collections.Generic;

public class SCMoveSet : ScriptableObject
{
    public IntegerRect NormalHitboxSpecs;
    public IntegerRect DuckHitboxSpecs;
    public SCAttack GroundNeutral;
    public SCAttack Dodge;

    //TODO: Expand as more attack options, combos, etc are added
    // Also need to update SCMoveSetEditingEditor when more moves are added
    public SCAttack GetAttackForInput(SCCharacterController.InputWrapper input)
    {
        if (input.DodgeBegin)
            return this.Dodge;
        if (input.AttackLightBegin)
            return this.GroundNeutral;
        return null;
    }
}
