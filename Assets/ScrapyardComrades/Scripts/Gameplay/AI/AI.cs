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
    public bool ChangedTarget;
    public IntegerVector OurPosition;
    public IntegerVector TargetPosition;
    public IntegerCollider OurCollider;
    public IntegerCollider TargetCollider;
    public bool OnGround;
    public bool TargetOnGround;
    public bool ExecutingMove;
    public bool InMoveCooldown;
    public bool HitStunned;
}

public class AI
{
    public AIState CurrentState;
    public AIState DefaultState;

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

    public void ResetAI()
    {
        this.CurrentState = this.DefaultState;
    }
}

public class SimpleAI : AI
{
    public SimpleAI(float attackStateRange, float pursuitRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown, int defenseDuration, float defenseChance, float walkToTargetDist, int interactDelay, SoundData.Key attractSoundKey)
    {
        AIState idleState = new IdleState(attractSoundKey);
        SimpleAttackState attackState = new SimpleAttackState(executeAttackRange, attackingPursuitTargetDist, attackStateCooldown, -1, defenseChance);
        AIState walkState = new WalkTowardState(walkToTargetDist, interactDelay); // Walk toward targets that are not alive
        SimpleDefenseState defenseState = new SimpleDefenseState(defenseDuration);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(attackState, true),
            new TargetWithinRangeTransition(attackState, 0, attackStateRange, true)
        }));
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(walkState, false),
            new TargetWithinRangeTransition(walkState, 0, attackStateRange, true)
        }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetChangedTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[]{
            new TargetAliveTransition(idleState, false),
            new TargetWithinRangeTransition(idleState, 0, executeAttackRange)
        }));
        attackState.AddTransition(new CustomTransition(attackState, defenseState));
        defenseState.AddTransition(new CustomTransition(defenseState, attackState));
        walkState.AddTransition(new NoTargetTransition(idleState));
        walkState.AddTransition(new TargetChangedTransition(idleState));
        walkState.AddTransition(new TargetAliveTransition(idleState, true));
        walkState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));

        this.DefaultState = idleState;
        this.CurrentState = idleState;
    }
}

public class GuardAI : AI
{
    public GuardAI(float attackStateRange, float pursuitRange, float executeChargeRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown, int defenseDuration, float defenseChance, float walkToTargetDist, int interactDelay, SoundData.Key attractSoundKey)
    {
        AIState idleState = new IdleState(attractSoundKey);
        GuardAttackState attackState = new GuardAttackState(executeChargeRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown, -1, defenseChance);
        AIState walkState = new WalkTowardState(walkToTargetDist, interactDelay); // Walk toward targets that are not alive
        SimpleDefenseState defenseState = new SimpleDefenseState(defenseDuration);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(attackState, true),
            new TargetWithinRangeTransition(attackState, 0, attackStateRange, true)
        }));
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(walkState, false),
            new TargetWithinRangeTransition(walkState, 0, attackStateRange, true)
        }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetChangedTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(idleState, false),
            new TargetWithinRangeTransition(idleState, 0, executeAttackRange)
        }));
        attackState.AddTransition(new CustomTransition(attackState, defenseState));
        defenseState.AddTransition(new CustomTransition(defenseState, attackState));
        walkState.AddTransition(new NoTargetTransition(idleState));
        walkState.AddTransition(new TargetChangedTransition(idleState));
        walkState.AddTransition(new TargetAliveTransition(idleState, true));
        walkState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));

        this.DefaultState = idleState;
        this.CurrentState = idleState;
    }
}

public class MidMutantAI : AI
{
    public MidMutantAI(float attackStateRange, float pursuitRange, float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown, int defenseDuration, float defenseChance, float walkToTargetDist, int interactDelay, SoundData.Key attractSoundKey, int executeStrongAttackRange = -1)
    {
        AIState idleState = new IdleState(attractSoundKey);
        MidMutantAttackState attackState = new MidMutantAttackState(jumpAtRangeFar, jumpAtRangeNear, executeAirAttackRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown, executeStrongAttackRange, defenseChance);
        AIState walkState = new WalkTowardState(walkToTargetDist, interactDelay); // Walk toward targets that are not alive
        SimpleDefenseState defenseState = new SimpleDefenseState(defenseDuration);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(attackState, true),
            new TargetWithinRangeTransition(attackState, 0, attackStateRange, true)
        }));
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(walkState, false),
            new TargetWithinRangeTransition(walkState, 0, attackStateRange, true)
        }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetChangedTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(idleState, false),
            new TargetWithinRangeTransition(idleState, 0, executeAttackRange)
        }));
        attackState.AddTransition(new CustomTransition(attackState, defenseState));
        defenseState.AddTransition(new CustomTransition(defenseState, attackState));
        walkState.AddTransition(new NoTargetTransition(idleState));
        walkState.AddTransition(new TargetChangedTransition(idleState));
        walkState.AddTransition(new TargetAliveTransition(idleState, true));
        walkState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));

        this.DefaultState = idleState;
        this.CurrentState = idleState;
    }
}

public class BossMilitaryAI : AI
{
    public BossMilitaryAI(float attackStateRange, float pursuitRange, float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown, int defenseDuration, float defenseChance, float executeDiveRange, float executeDiveAloneRange, int airFramesForAOE, float walkToTargetDist, int interactDelay, SoundData.Key attractSoundKey)
    {
        AIState idleState = new IdleState(attractSoundKey);
        BossMilitaryAttackState attackState = new BossMilitaryAttackState(jumpAtRangeFar, jumpAtRangeNear, executeAirAttackRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown, -1, defenseChance, executeDiveRange, executeDiveAloneRange, airFramesForAOE);
        AIState walkState = new WalkTowardState(walkToTargetDist, interactDelay); // Walk toward targets that are not alive
        SimpleDefenseState defenseState = new SimpleDefenseState(defenseDuration);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(attackState, true),
            new TargetWithinRangeTransition(attackState, 0, attackStateRange, true)
        }));
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(walkState, false),
            new TargetWithinRangeTransition(walkState, 0, attackStateRange, true)
        }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetChangedTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(idleState, false),
            new TargetWithinRangeTransition(idleState, 0, executeAttackRange)
        }));
        attackState.AddTransition(new CustomTransition(attackState, defenseState));
        defenseState.AddTransition(new CustomTransition(defenseState, attackState));
        walkState.AddTransition(new NoTargetTransition(idleState));
        walkState.AddTransition(new TargetChangedTransition(idleState));
        walkState.AddTransition(new TargetAliveTransition(idleState, true));
        walkState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));

        this.DefaultState = idleState;
        this.CurrentState = idleState;
    }
}

public class MutantAI : AI
{
    public MutantAI(float attackStateRange, float pursuitRange, float executeChargeRange, float jumpAtRangeFar, float jumpAtRangeNear, float executeAirAttackRange, float executeAttackRange, float attackingPursuitTargetDist, int attackStateCooldown, int defenseDuration, float defenseChance, float walkToTargetDist, int interactDelay, SoundData.Key attractSoundKey)
    {
        AIState idleState = new IdleState(attractSoundKey);
        MutantAttackState attackState = new MutantAttackState(jumpAtRangeFar, jumpAtRangeNear, executeAirAttackRange, executeChargeRange, executeAttackRange, attackingPursuitTargetDist, attackStateCooldown);
        AIState walkState = new WalkTowardState(walkToTargetDist, interactDelay); // Walk toward targets that are not alive
        SimpleDefenseState defenseState = new SimpleDefenseState(defenseDuration);

        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(attackState, true),
            new TargetWithinRangeTransition(attackState, 0, attackStateRange, true)
        }));
        idleState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(walkState, false),
            new TargetWithinRangeTransition(walkState, 0, attackStateRange, true)
        }));
        attackState.AddTransition(new NoTargetTransition(idleState));
        attackState.AddTransition(new TargetChangedTransition(idleState));
        attackState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));
        attackState.AddTransition(new ANDTransition(new AIStateTransition[] {
            new TargetAliveTransition(idleState, false),
            new TargetWithinRangeTransition(idleState, 0, executeAttackRange)
        }));
        attackState.AddTransition(new CustomTransition(attackState, defenseState));
        defenseState.AddTransition(new CustomTransition(defenseState, attackState));
        walkState.AddTransition(new NoTargetTransition(idleState));
        walkState.AddTransition(new TargetChangedTransition(idleState));
        walkState.AddTransition(new TargetAliveTransition(idleState, true));
        walkState.AddTransition(new TargetWithinRangeTransition(idleState, pursuitRange));

        this.DefaultState = idleState;
        this.CurrentState = idleState;
    }
}
