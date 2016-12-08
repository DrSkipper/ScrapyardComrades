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
    public AINode Root;

    
    public virtual AIOutput RunAI(AIInput input)
    {
        return new AIOutput();
    }
}
