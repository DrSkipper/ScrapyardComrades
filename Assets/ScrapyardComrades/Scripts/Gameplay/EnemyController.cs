using UnityEngine;

public class EnemyController : SCCharacterController
{
    public override void Awake()
    {
        base.Awake();
        GlobalEvents.Notifier.Listen(PlayerSpawnedEvent.NAME, this, playerSpawned);
    }
    public override void OnSpawn()
    {
        base.OnSpawn();

        //TODO: Data-drive which AI to use, as well as parameters
        _ai = new SimpleAI(500, 700, 60, 20);
    }

    public override InputWrapper GatherInput()
    {
        _target = PlayerReference.Collider;
        EnemyInput controlInput = new EnemyInput(_ai.RunAI(gatherAiInput()), _previousInput);
        _previousInput = controlInput;
        return controlInput;
    }

    /**
     * Private
     */
    private AI _ai;
    private IntegerCollider _target;
    private EnemyInput _previousInput;

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

    private void playerSpawned(LocalEventNotifier.Event e)
    {
        _target = (e as PlayerSpawnedEvent).PlayerObject.GetComponent<IntegerCollider>();
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

        public EnemyInput(AIOutput output, EnemyInput previousInput)
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
            this.Interact = false;
            this.PausePressed = false;
        }
    }
}
