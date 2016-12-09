using UnityEngine;

public struct AIOutput
{
    public int MovementDirection;
    public bool Jump;
    public bool Attack;
}

public struct AIInput
{
    public bool HasTarget;
    public IntegerVector OurPosition;
    public IntegerVector TargetPosition;
    public IntegerCollider OurCollider;
    public IntegerCollider TargetCollider;
    public bool OnGround;
    public bool ExecutingMove;
    public bool HitStunned;
}

public class AI
{
    public AIState CurrentState;

    public AIOutput RunAI(AIInput input)
    {
        if (this.CurrentState == null)
            return new AIOutput();
        AIState newState = this.CurrentState.CheckTransitions(input);
        if (newState != this.CurrentState)
        {
            this.CurrentState.ExitState();
            newState.EnterState();
            this.CurrentState = newState;
        }
        return this.CurrentState.UpdateState(input);
    }
}

public class SimpleAI : AI
{
    public SimpleAI(float attackStateRange, float pursuitRange, float executeAttackRange, float attackingPursuitTargetDist)
    {
        AIState idleState = new IdleState();
        AIState attackState = new SimpleAttackState(executeAttackRange, attackingPursuitTargetDist);
        idleState.AddTransition(new TargetWithinRangeTransition(attackState, 0, attackStateRange));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        this.CurrentState = idleState;
    }
}
