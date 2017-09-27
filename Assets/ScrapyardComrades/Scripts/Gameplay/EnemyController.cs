using UnityEngine;
using System.Collections.Generic;

public class EnemyController : SCCharacterController
{
    public AIType AI = AIType.Simple;
    public TargetType Targets = TargetType.Player;
    public LayerMask TargetLayers;
    public LayerMask SecondaryTargetLayers;
    public string SecondaryTargetTag = "Heart";

    [System.Serializable]
    public enum AIType
    {
        Simple,
        Guard,
        Office
    }

    [System.Serializable]
    public enum TargetType
    {
        Player,
        SpecifiedLayers
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        this.Damagable.Health = this.Damagable.MaxHealth;

        if (_ai == null)
        {
            switch (this.AI)
            {
                default:
                case AIType.Simple:
                    _attackStateRange = SIMPLE_ATTACK_RANGE;
                    _ai = new SimpleAI(SIMPLE_ATTACK_RANGE, 425, 75, 48, 4, WALK_TO_TARGET_DIST, INTERACT_DELAY);
                    break;
                case AIType.Guard:
                    _attackStateRange = GUARD_ATTACK_RANGE;
                    _ai = new GuardAI(GUARD_ATTACK_RANGE, 400, 156, 114, 128, 8, WALK_TO_TARGET_DIST, INTERACT_DELAY);
                    break;
                case AIType.Office:
                    _attackStateRange = OFFICE_ATTACK_RANGE;
                    _ai = new OfficeAI(OFFICE_ATTACK_RANGE, 425, 200, 145, 180, 100, 60, 4, WALK_TO_TARGET_DIST, INTERACT_DELAY);
                    break;
            }
        }
        else
        {
            _ai.ResetAI();
        }
    }

    public override InputWrapper GatherInput()
    {
        IntegerCollider target = null;
        bool targetAlive = false;

        switch (this.Targets)
        {
            default:
            case TargetType.Player:
                target = PlayerReference.Collider;
                targetAlive = PlayerReference.IsAlive;
                break;
            case TargetType.SpecifiedLayers:
                _targetsInRange = CollisionManager.Instance.GetCollidersInRange(new IntegerRect(this.integerPosition, new IntegerVector(_attackStateRange * 2, _attackStateRange * 2)), this.TargetLayers, null, _targetsInRange);

                _targetsInRange.Remove(this.Hurtbox);
                target = findClosest();
                if (target != null)
                    targetAlive = true;
                break;
        }

        // Also check for secondary target
        _targetsInRange = CollisionManager.Instance.GetCollidersInRange(new IntegerRect(this.integerPosition, new IntegerVector(_attackStateRange * 2, _attackStateRange * 2)), this.SecondaryTargetLayers, this.SecondaryTargetTag, _targetsInRange);
        IntegerCollider secondaryTarget = findClosest();

        if (target == null)
        {
            target = secondaryTarget;
        }

        // If both regular target and secondary target exist, see if we have line of sight to regular target
        else if (secondaryTarget != null)
        {
            IntegerVector targetPosition = (Vector2)target.transform.position;
            IntegerVector ourPosition = this.integerPosition;
            IntegerVector diff = targetPosition - new IntegerVector(ourPosition.X, this.Hurtbox.Bounds.Max.Y);
            if (CollisionManager.Instance.RaycastFirst(ourPosition, ((Vector2)diff).normalized, ((Vector2)diff).magnitude, 1 << TargetWithinRangeTransition.LineOfSightBlocker).Collided)
            {
                // Secondary targets are considered Not Alive
                target = secondaryTarget;
                targetAlive = false;
            }
            
        }

        _input.ApplyValues(_ai.RunAI(gatherAiInput(target, targetAlive)));
        return _input;
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        if (_targetsInRange != null)
            _targetsInRange.Clear();
    }

    /**
     * Private
     */
    private AI _ai;
    private EnemyInput _input;
    private int _attackStateRange;
    private List<IntegerCollider> _targetsInRange;
    private IntegerCollider _prevTarget;

    private const int SIMPLE_ATTACK_RANGE = 250;
    private const int GUARD_ATTACK_RANGE = 275;
    private const int OFFICE_ATTACK_RANGE = 280;
    private const float WALK_TO_TARGET_DIST = 16;
    private const int INTERACT_DELAY = 25;

    private AIInput gatherAiInput(IntegerCollider target, bool targetAlive)
    {
        AIInput aiInput = new AIInput();
        aiInput.HasTarget = target != null;
        aiInput.TargetAlive = targetAlive;
        aiInput.ChangedTarget = target != _prevTarget;
        _prevTarget = target;
        aiInput.OurPosition = (Vector2)this.transform.position;
        aiInput.TargetPosition = target != null ? (Vector2)target.transform.position : Vector2.zero;
        aiInput.OurCollider = this.Hurtbox;
        aiInput.TargetCollider = target;
        aiInput.OnGround = this.OnGround;
        aiInput.ExecutingMove = this.ExecutingMove;
        aiInput.InMoveCooldown = this.InMoveCooldown;
        aiInput.HitStunned = this.HitStunned;

        if (targetAlive)
        {
            SCCharacterController targetController = target.GetComponent<SCCharacterController>();
            aiInput.TargetOnGround = targetController.OnGround && Mathf.RoundToInt(targetController.Velocity.y) <= 0;
        }
        else
        {
            aiInput.TargetOnGround = false;
        }
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
        public bool LookUp { get; private set; }
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
            this.DodgeBegin = output.Dodge;
            this.DodgeHeld = output.Dodge;
            this.Duck = false;
            this.AttackLightBegin = output.Attack;
            this.AttackLightHeld = output.Attack;
            this.AttackStrongBegin = output.AttackStrong;
            this.AttackStrongHeld = output.AttackStrong;
            this.UseItem = false;
            this.Interact = output.Interact;
            this.PausePressed = false;
        }
    }

    private IntegerCollider findClosest()
    {
        IntegerCollider target = null;
        int dist = int.MaxValue;
        for (int i = 0; i < _targetsInRange.Count; ++i)
        {
            int d = Mathf.RoundToInt(Vector2.Distance(_targetsInRange[i].transform.position, this.transform.position));
            if (target == null || d < dist)
            {
                dist = d;
                target = _targetsInRange[i];
            }
        }
        return target;
    }
}
