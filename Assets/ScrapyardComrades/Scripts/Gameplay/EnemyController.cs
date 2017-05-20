using UnityEngine;

public class EnemyController : SCCharacterController
{
    public override void OnSpawn()
    {
        base.OnSpawn();

        //TODO - Data-drive health
        this.Damagable.Health = this.Damagable.MaxHealth;

        //TODO: Data-drive which AI to use, as well as parameters
        _ai = new SimpleAI(250, 450, 75, 30);
    }

    public override InputWrapper GatherInput()
    {
        _input.ApplyValues(_ai.RunAI(gatherAiInput(PlayerReference.Collider, PlayerReference.IsAlive)));
        return _input;
    }

    /**
     * Private
     */
    private AI _ai;
    private EnemyInput _input;

    private AIInput gatherAiInput(IntegerCollider target, bool targetAlive)
    {
        AIInput aiInput = new AIInput();
        aiInput.HasTarget = target != null;
        aiInput.TargetAlive = targetAlive;
        aiInput.OurPosition = (Vector2)this.transform.position;
        aiInput.TargetPosition = target != null ? (Vector2)target.transform.position : Vector2.zero;
        aiInput.OurCollider = this.Hurtbox;
        aiInput.TargetCollider = target;
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

        public void ApplyValues(AIOutput output)
        {
            this.MovementAxis = output.MovementDirection;
            this.JumpBegin = output.Jump;
            this.JumpHeld = output.Jump;
            this.DodgeBegin = false;
            this.DodgeHeld = false;
            this.Duck = false;
            this.AttackLightBegin = output.Attack;
            this.AttackLightHeld = output.Attack;
            this.AttackStrongBegin = false;
            this.AttackStrongHeld = false;
            this.UseItem = false;
            this.Interact = output.Interact;
            this.PausePressed = false;
        }
    }
}
