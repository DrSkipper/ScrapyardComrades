using UnityEngine;
using System.Collections.Generic;

public class AIState
{
    public virtual void EnterState()
    {
    }

    public virtual AIOutput UpdateState(AIInput input)
    {
        return new AIOutput();
    }

    public virtual void ExitState()
    {
    }

    public AIState CheckTransitions(AIInput input)
    {
        if (this.Transitions != null)
        {
            for (int i = 0; i < this.Transitions.Count; ++i)
            {
                if (this.Transitions[i].ShouldTransition(input))
                {
                    return this.Transitions[i].Destination;
                }
            }
        }
        return this;
    }

    public void AddTransition(AIStateTransition transition)
    {
        if (this.Transitions == null)
            this.Transitions = new List<AIStateTransition>();
        this.Transitions.Add(transition);
    }

    /**
     * Private
     */
    private List<AIStateTransition> Transitions;
}

public class IdleState : AIState
{
    public IdleState(SoundData.Key exitSoundKey)
    {
        _exitSoundKey = exitSoundKey;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        output.Interact = true;
        return output;
    }

    public override void ExitState()
    {
        SoundManager.Play(_exitSoundKey);
    }

    /**
     * Private
     */
    private SoundData.Key _exitSoundKey;
}

public class WalkTowardState : AIState
{
    public WalkTowardState(float pursuitToDist, int interactDelay)
    {
        _pursuitToDist = pursuitToDist;
        _interactDelay = interactDelay;
        _t = _interactDelay;
    }

    public override void EnterState()
    {
        _t = _interactDelay;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        if (Mathf.Abs(input.OurPosition.X - input.TargetPosition.X) < _pursuitToDist)
        {
            output.MovementDirection = 0;
            if (_t <= 0)
            {
                _t = _interactDelay;
                output.Interact = true;
            }
            else
                --_t;
        }
        else
        {
            output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));
        }
        return output;
    }

    private float _pursuitToDist;
    private int _interactDelay;
    private int _t;
}

public class SimpleAttackState : AIState, CustomTransitionState
{
    public SimpleAttackState(float executeAttackRange, float pursuitToDist, int cooldown, int executeStrongAttackRange = -1, float defenseStateChance = 0.5f)
    {
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
        _executeStrongAttackRange = executeStrongAttackRange;
        _defenseStateChance = defenseStateChance;
    }

    public override void EnterState()
    {
        _cooldown = 0;
        _canCustomTransition = false;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        _canCustomTransition = false;

        if (_cooldown <= 0)
        {
            bool attack = false;
            output.Jump = input.TargetOnGround && input.TargetCollider.Bounds.Min.Y > input.OurCollider.Bounds.Min.Y + 8;

            if (input.OnGround)
            {
                float d = Vector2.Distance(input.OurPosition, input.TargetPosition);
                if (d <= _executeAttackRange)
                    output.Attack = true;
                else if (d <= _executeStrongAttackRange)
                    output.AttackStrong = true;

                if ((output.Attack || output.AttackStrong) && !input.InMoveCooldown && !input.ExecutingMove)
                {
                    attack = true;
                    _cooldown = _cooldownAmt;
                }
            }

            if (!attack && Mathf.Abs(input.OurPosition.X - input.TargetPosition.X) < _pursuitToDist)
                output.MovementDirection = 0;
            else
                output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));
        }
        else if (!input.ExecutingMove)
        {
            --_cooldown;
            _canCustomTransition = _cooldown == 0;
        }
        return output;
    }

    // Allow transition to defensive state if conditions are met
    public bool CanCustomTransition { get { return _canCustomTransition && Random.Range(0.0f, 1.0f) < _defenseStateChance; } }

    private float _executeAttackRange;
    private float _executeStrongAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
    private float _defenseStateChance;
    private bool _canCustomTransition;
}

public class SimpleDefenseState : AIState, CustomTransitionState
{
    public bool CanCustomTransition { get { return _cooldown == 0; } }

    public SimpleDefenseState(int duration)
    {
        _duration = duration;
        _cooldown = duration;
    }

    public override void EnterState()
    {
        _cooldown = _duration;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        if (_cooldown > 0)
            --_cooldown;
        return new AIOutput();
    }

    private int _duration;
    private int _cooldown;
}

public class GuardAttackState : AIState, CustomTransitionState
{
    public GuardAttackState(float executeChargeRange, float executeAttackRange, float pursuitToDist, int cooldown, int executeStrongAttackRange = -1, float defenseStateChance = 0.5f)
    {
        _executeChargeRange = executeChargeRange;
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
        _defenseStateChance = defenseStateChance;
    }

    public override void EnterState()
    {
        _cooldown = 0;
        _canCustomTransition = false;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        _canCustomTransition = false;
        if (_cooldown <= 0)
        {
            float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);
            bool attack = false;

            if (input.OnGround && !input.InMoveCooldown && !input.ExecutingMove)
            {
                if (d <= _executeAttackRange)
                {
                    attack = true;
                    output.Attack = input.TargetPosition.Y <= input.OurPosition.Y;
                    output.AttackStrong = !output.Attack;
                }
                else if (d <= _executeChargeRange)
                {
                    attack = true;
                    output.Dodge = true;
                }

                if (attack)
                    _cooldown = _cooldownAmt;
            }

            if (d < _pursuitToDist && !attack)
                output.MovementDirection = 0;
            else
                output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));
        }
        else if (!input.ExecutingMove)
        {
            --_cooldown;
            _canCustomTransition = _cooldown == 0;
        }
        return output;
    }
    
    // Allow transition to defensive state if conditions are met
    public bool CanCustomTransition { get { return _canCustomTransition && Random.Range(0.0f, 1.0f) < _defenseStateChance; } }

    private float _executeChargeRange;
    private float _executeAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
    private float _defenseStateChance;
    private bool _canCustomTransition;
}

public class MidMutantAttackState : SimpleAttackState
{
    public MidMutantAttackState(float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float pursuitToDist, int cooldown, int executeStrongAttackRange = -1, float defenseStateChance = 0.5f) : base(executeAttackRange, pursuitToDist, cooldown, executeStrongAttackRange, defenseStateChance)
    {
        _jumpAtRangeFar = jumpAtRangeFar;
        _jumpAtRangeNear = jumpAtRangeNear;
        _executeAirAttackRange = executeAttackRange;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = base.UpdateState(input);
        float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);

        if (input.OnGround)
        {
            if (!output.Jump && d < _jumpAtRangeFar && d > _jumpAtRangeNear)
                output.Jump = true;
        }

        else if (!output.Attack && !output.AttackStrong)
        {
            if (d < _executeAirAttackRange)
                output.Attack = true;
        }

        return output;
    }

    private float _jumpAtRangeFar;
    private float _jumpAtRangeNear;
    private float _executeAirAttackRange;
}

public class MutantAttackState : AIState, CustomTransitionState
{
    public MutantAttackState(float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeChargeRange, float executeAttackRange, float pursuitToDist, int cooldown, float defenseStateChance = 0.5f)
    {
        _jumpAtRangeFar = jumpAtRangeFar;
        _jumpAtRangeNear = jumpAtRangeNear;
        _executeAirAttackRange = executeAttackRange;
        _executeChargeRange = executeChargeRange;
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
        _defenseStateChance = defenseStateChance;
    }

    public override void EnterState()
    {
        _cooldown = 0;
        _canCustomTransition = false;
        _lastAttackWasCharge = false;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        _canCustomTransition = false;

        if (!input.ExecutingMove)
            _charging = false;

        float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);
        if (_cooldown <= 0)
        {
            bool attack = false;

            if (input.OnGround && !output.Jump && d < _jumpAtRangeFar && d > _jumpAtRangeNear && input.TargetCollider.Bounds.Min.Y > input.OurCollider.Bounds.Min.Y + 2)
            {
                output.Jump = true;
            }
            else if (!input.InMoveCooldown && (!input.ExecutingMove || _charging))
            {
                if (input.OnGround)
                {
                    if (d <= _executeAttackRange)
                    {
                        attack = true;
                        output.Attack = true;
                    }
                    else if (!_charging && d <= _executeChargeRange)
                    {
                        attack = true;
                        output.Dodge = true;
                        _charging = true;
                    }
                }
                else if (d <= _executeAirAttackRange)
                {
                    attack = true;
                    output.Attack = true;
                }

                if (attack)
                    _cooldown = _cooldownAmt;
            }

            if (d < _pursuitToDist && !attack)
                output.MovementDirection = 0;
            else
                output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));
        }
        else
        {
            if (_lastAttackWasCharge && input.OnGround && d <= _executeAttackRange)
            {
                output.Attack = true;
                _cooldown = _cooldownAmt;
            }
            else if (!input.ExecutingMove)
            {
                --_cooldown;
                _canCustomTransition = _cooldown == 0;
            }
        }

        _lastAttackWasCharge = _charging;
        return output;
    }

    // Allow transition to defensive state if conditions are met
    public bool CanCustomTransition { get { return _canCustomTransition && Random.Range(0.0f, 1.0f) < _defenseStateChance; } }

    private bool _charging;
    private float _executeChargeRange;
    private float _executeAttackRange;
    private float _jumpAtRangeFar;
    private float _jumpAtRangeNear;
    private float _executeAirAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
    private float _defenseStateChance;
    private bool _canCustomTransition;
    private bool _lastAttackWasCharge;
}
