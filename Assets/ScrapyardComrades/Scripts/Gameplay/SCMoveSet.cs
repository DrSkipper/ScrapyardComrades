using UnityEngine;

public class SCMoveSet : ScriptableObject
{
    [System.Serializable]
    public enum MoveInput
    {
        None,
        Neutral,
        Strong,
        Dodge
    }

    public IntegerRect NormalHitboxSpecs;
    public IntegerRect DuckHitboxSpecs;
    public SCAttack GroundNeutral;
    public SCAttack GroundStrong;
    public SCAttack GroundDodge;
    public SCAttack AirNeutral;
    public SCAttack AirStrong;
    public SCAttack AirDodge;

    public SCAttack ExtraRefA;
    public SCAttack ExtraRefB;
    public SCAttack ExtraRefC;

    //TODO: Expand as more attack options, etc are added
    // Also need to update SCMoveSetEditingEditor when more moves are added
    public SCAttack GetAttackForInput(SCCharacterController.InputWrapper input, SCCharacterController character, int categoryConstraints = int.MaxValue)
    {
        if (character.OnGround)
        {
            if (input.DodgeBegin && validMove(this.GroundDodge, categoryConstraints))
                return this.GroundDodge;
            if (input.AttackStrongBegin && validMove(this.GroundStrong, categoryConstraints))
                return this.GroundStrong;
            if (input.AttackLightBegin && validMove(this.GroundNeutral, categoryConstraints))
                return this.GroundNeutral;
        }
        else
        {
            if (input.DodgeBegin && validMove(this.AirDodge, categoryConstraints))
                return this.AirDodge;
            if (input.AttackStrongBegin && validMove(this.AirStrong, categoryConstraints))
                return this.AirStrong;
            if (input.AttackLightBegin && validMove(this.AirNeutral, categoryConstraints))
                return this.AirNeutral;
        }
        return null;
    }

    public SCAttack GetComboMove(SCCharacterController.InputWrapper input, SCAttack previousMove, MoveInput bufferedInput)
    {
        if (previousMove.Combos != null)
        {
            for (int i = 0; i < previousMove.Combos.Length; ++i)
            {
                if (checkInput(input, previousMove.Combos[i].Input))
                    return previousMove.Combos[i].ComboMove;
            }
            
            // If didn't find one, lets try the buffered input
            for (int i = 0; i < previousMove.Combos.Length; ++i)
            {
                if (previousMove.Combos[i].Input == bufferedInput)
                    return previousMove.Combos[i].ComboMove;
            }
        }
        return null;
    }

    public static MoveInput GetCurrentMoveInput(SCCharacterController.InputWrapper input)
    {
        if (input.DodgeBegin)
            return MoveInput.Dodge;
        if (input.AttackStrongBegin)
            return MoveInput.Strong;
        if (input.AttackLightBegin)
            return MoveInput.Neutral;
        return MoveInput.None;
    }

    private static bool checkInput(SCCharacterController.InputWrapper input, MoveInput check)
    {
        switch (check)
        {
            default:
            case MoveInput.None:
                return false;
            case MoveInput.Neutral:
                return input.AttackLightBegin;
            case MoveInput.Strong:
                return input.AttackStrongBegin;
            case MoveInput.Dodge:
                return input.DodgeBegin;
        }
    }

    private static bool validMove(SCAttack move, int categoryConstraints)
    {
        return move != null && ((int)move.Category & categoryConstraints) != 0;
    }
}
