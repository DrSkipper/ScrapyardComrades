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
    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        output.Interact = true;
        return output;
    }
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

public class SimpleAttackState : AIState
{
    public SimpleAttackState(float executeAttackRange, float pursuitToDist, int cooldown, int executeStrongAttackRange = -1)
    {
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
        _executeStrongAttackRange = executeStrongAttackRange;
    }

    public override void EnterState()
    {
        _cooldown = 0;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
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
        }
        return output;
    }

    private float _executeAttackRange;
    private float _executeStrongAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
}

public class SimpleDefenseState : AIState, CooldownState
{
    public int Cooldown { get { return _cooldown; } }

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

public class GuardAttackState : AIState
{
    public GuardAttackState(float executeChargeRange, float executeAttackRange, float pursuitToDist, int cooldown)
    {
        _executeChargeRange = executeChargeRange;
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
    }

    public override void EnterState()
    {
        _cooldown = 0;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
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
        }
        return output;
    }

    private float _executeChargeRange;
    private float _executeAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
}

public class MidMutantAttackState : SimpleAttackState
{
    public MidMutantAttackState(float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float pursuitToDist, int cooldown, int executeStrongAttackRange = -1) : base(executeAttackRange, pursuitToDist, cooldown, executeStrongAttackRange)
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

public class MutantAttackState : AIState
{
    public MutantAttackState(float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeChargeRange, float executeAttackRange, float pursuitToDist, int cooldown)
    {
        _jumpAtRangeFar = jumpAtRangeFar;
        _jumpAtRangeNear = jumpAtRangeNear;
        _executeAirAttackRange = executeAttackRange;
        _executeChargeRange = executeChargeRange;
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
        _cooldownAmt = cooldown;
    }

    public override void EnterState()
    {
        _cooldown = 0;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();

        if (!input.ExecutingMove)
            _charging = false;

        if (_cooldown <= 0)
        {
            float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);
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
                else if (d <= this._executeAirAttackRange)
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
        else if (!input.ExecutingMove)
        {
            --_cooldown;
        }
        return output;
    }

    private bool _charging;
    private float _executeChargeRange;
    private float _executeAttackRange;
    private float _jumpAtRangeFar;
    private float _jumpAtRangeNear;
    private float _executeAirAttackRange;
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
}
