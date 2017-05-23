
public class PlayerRestrictedController : PlayerController
{
    public struct PlayerRestrictedInput : InputWrapper
    {
        public int MovementAxis { get { return GameplayInput.MovementAxis; } }
        public bool JumpBegin { get { return false; } }
        public bool JumpHeld { get { return false; } }
        public bool DodgeBegin { get { return false; } }
        public bool DodgeHeld { get { return false; } }
        public bool Duck { get { return false; } }
        public bool AttackLightBegin { get { return false; } }
        public bool AttackLightHeld { get { return false; } }
        public bool AttackStrongBegin { get { return false; } }
        public bool AttackStrongHeld { get { return false; } }
        public bool UseItem { get { return false; } }
        public bool Interact { get { return GameplayInput.Interact; } }
        public bool PausePressed { get { return GameplayInput.PausePressed; } }

        public static PlayerRestrictedInput Reference = new PlayerRestrictedInput();
    }

    public override InputWrapper GatherInput()
    {
        return PlayerRestrictedInput.Reference;
    }
}
