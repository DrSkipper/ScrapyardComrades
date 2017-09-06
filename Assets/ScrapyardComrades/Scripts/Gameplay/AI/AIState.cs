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

public class SimpleAttackState : AIState
{
    public SimpleAttackState(float executeAttackRange, float pursuitToDist, int cooldown)
    {
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
            bool attack = false;
            output.Jump = input.TargetOnGround && input.TargetCollider.Bounds.Min.Y > input.OurCollider.Bounds.Min.Y + 8;

            if (input.OnGround && Vector2.Distance(input.OurPosition, input.TargetPosition) <= _executeAttackRange)
            {
                output.Attack = true;
                if (!input.InMoveCooldown && !input.ExecutingMove)
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
    private float _pursuitToDist;
    private int _cooldown;
    private int _cooldownAmt;
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

public class OfficeAttackState : SimpleAttackState
{
    public OfficeAttackState(float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float pursuitToDist, int cooldown) : base(executeAttackRange, pursuitToDist, cooldown)
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

        else if (!output.Attack)
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
