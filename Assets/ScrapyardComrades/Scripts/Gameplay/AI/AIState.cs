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
    public SimpleAttackState(float executeAttackRange, float pursuitToDist)
    {
        _executeAttackRange = executeAttackRange;
        _pursuitToDist = pursuitToDist;
    }

    public override AIOutput UpdateState(AIInput input)
    {
        AIOutput output = new AIOutput();
        float d = Mathf.Abs(input.OurPosition.X - input.TargetPosition.X);
        if (d < _pursuitToDist)
            output.MovementDirection = 0;
        else
            output.MovementDirection = Mathf.RoundToInt(Mathf.Sign(input.TargetPosition.X - input.OurPosition.X));
        output.Jump = input.TargetCollider.Bounds.Min.Y > input.OurCollider.Bounds.Max.Y;
        output.Attack = Vector2.Distance(input.OurPosition, input.TargetPosition) <= _executeAttackRange;
        return output;
    }

    private float _executeAttackRange;
    private float _pursuitToDist;
}

