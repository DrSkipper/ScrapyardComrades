using UnityEngine;

public struct AIOutput
{
    public int MovementDirection;
    public bool Jump;
    public bool Attack;
    public bool AttackStrong;
    public bool Dodge;
    public bool Interact;
}

public struct AIInput
{
    public bool HasTarget;
    public bool TargetAlive;
    public IntegerVector OurPosition;
    public IntegerVector TargetPosition;
    public IntegerCollider OurCollider;
    public IntegerCollider TargetCollider;
    public bool OnGround;
    public bool ExecutingMove;
    public bool InMoveCooldown;
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
    public SimpleAI(float attackStateRange, float pursuitRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown)
    {
        AIState idleState = new IdleState();
        AIState attackState = new SimpleAttackState(executeAttackRange, attackingPursuitTargetDist, attackStateCooldown);
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] { new TargetAliveTransition(attackState, true), new TargetWithinRangeTransition(attackState, 0, attackStateRange) }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[]{ new TargetAliveTransition(idleState, false), new TargetWithinRangeTransition(idleState, 0, executeAttackRange) }));
        this.CurrentState = idleState;
    }
}

public class GuardAI : AI
{
    public GuardAI(float attackStateRange, float pursuitRange, float executeChargeRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown)
    {
        AIState idleState = new IdleState();
        AIState attackState = new GuardAttackState(executeChargeRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] { new TargetAliveTransition(attackState, true), new TargetWithinRangeTransition(attackState, 0, attackStateRange) }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] { new TargetAliveTransition(idleState, false), new TargetWithinRangeTransition(idleState, 0, executeAttackRange) }));
        this.CurrentState = idleState;
    }
}

public class OfficeAI : AI
{
    public OfficeAI(float attackStateRange, float pursuitRange, float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown)
    {
        AIState idleState = new IdleState();
        AIState attackState = new OfficeAttackState(jumpAtRangeFar, jumpAtRangeNear, executeAirAttackRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown);
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] { new TargetAliveTransition(attackState, true), new TargetWithinRangeTransition(attackState, 0, attackStateRange) }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] { new TargetAliveTransition(idleState, false), new TargetWithinRangeTransition(idleState, 0, executeAttackRange) }));
        this.CurrentState = idleState;
    }
}
