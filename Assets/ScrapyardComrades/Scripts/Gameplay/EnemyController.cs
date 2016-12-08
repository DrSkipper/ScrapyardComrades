using UnityEngine;

public class EnemyController : SCCharacterController
{
    public enum EnemyState
    {
        Idle
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        //TODO: Data-drive which AI to use
        _ai = new AI();
    }

    public override InputWrapper GatherInput()
    {
        return new EnemyInput(_state, _intendedFacing);
    }

    /**
     * Private
     */
    private EnemyState _state;
    private Facing _intendedFacing;
    private AI _ai;
    private IntegerCollider _target;

    private AIInput gatherAiInput()
    {
        AIInput aiInput = new AIInput();
        aiInput.HasTarget = _target != null;
        aiInput.OurPosition = (Vector2)this.transform.position;
        aiInput.TargetPosition = _target != null ? (Vector2)_target.transform.position : Vector2.zero;
        aiInput.OurCollider = this.Hurtbox;
        aiInput.TargetCollider = _target;
        aiInput.OnGround = this.OnGround;
        aiInput.ExecutingMove = this.ExecutingMove;
        aiInput.HitStunned = this.HitStunned;
        return aiInput;
    }

    private struct EnemyInput : InputWrapper
    {
        public int MovementAxis { get; private set; }
        public bool JumpBegin { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DodgeBegin { get; private set; }
        public bool DodgeHeld { get; private set; }
        public bool Duck { get; private set; }
        public bool AttackLightBegin { get; private set; }
        public bool AttackLightHeld { get; private set; }
        public bool AttackStrongBegin { get; private set; }
        public bool AttackStrongHeld { get; private set; }
        public bool UseItem { get; private set; }
        public bool Interact { get; private set; }
        public bool PausePressed { get; private set; }

        public EnemyInput(EnemyState state, Facing intendedFacing)
        {
            this.PausePressed = false;

            switch (state)
            {
                default:
                case EnemyState.Idle:
                    this.MovementAxis = 0;
                    this.JumpBegin = false;
                    this.JumpHeld = false;
                    this.DodgeBegin = false;
                    this.DodgeHeld = false;
                    this.Duck = false;
                    this.AttackLightBegin = false;
                    this.AttackLightHeld = false;
                    this.AttackStrongBegin = false;
                    this.AttackStrongHeld = false;
                    this.UseItem = false;
                    this.Interact = false;
                    break;
            }
        }
    }
}
