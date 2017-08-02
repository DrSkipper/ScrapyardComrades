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
            float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);
            if (d < _pursuitToDist)
                output.MovementDirection = 0;
            else
                output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));

            output.Jump = input.TargetCollider.Bounds.Min.Y > input.OurCollider.Bounds.Max.Y;

            if (Vector2.Distance(input.OurPosition, input.TargetPosition) <= _executeAttackRange)
            {
                output.Attack = true;
                if (!input.InMoveCooldown && !input.ExecutingMove)
                    _cooldown = _cooldownAmt;
            }
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
            if (d < _pursuitToDist)
                output.MovementDirection = 0;
            else
                output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));

            if (input.OnGround && !input.InMoveCooldown && !input.ExecutingMove)
            {
                //d = Vector2.Distance(input.OurPosition, input.TargetPosition);
                if (d <= _executeAttackRange)
                {
                    output.Attack = input.TargetPosition.Y <= input.OurPosition.Y;
                    output.AttackStrong = !output.Attack;
                    _cooldown = _cooldownAmt;
                }
                else if (d <= _executeChargeRange)
                {
                    output.Dodge = true;
                    _cooldown = _cooldownAmt;
                }
            }
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
