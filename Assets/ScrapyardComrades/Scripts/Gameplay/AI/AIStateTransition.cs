using UnityEngine;

public interface AIStateTransition
{
    AIState Destination { get; }
    bool ShouldTransition(AIInput input);
}

public class TargetWithinRangeTransition : AIStateTransition
{
    public AIState Destination { get; private set; }
    public float MinDistanceForTransition { get; private set; }
    public float MaxDistanceForTransition { get; private set; }
    public bool RequiresLineOfSight { get; private set; }
    public const int LineOfSightBlocker = 8;

    public TargetWithinRangeTransition(AIState destination, float minDistanceForTransition, float maxDistanceForTransition = -1, bool requiresLineOfSight = false)
    {
        this.Destination = destination;
        this.MinDistanceForTransition = minDistanceForTransition;
        this.MaxDistanceForTransition = maxDistanceForTransition;
        this.RequiresLineOfSight = requiresLineOfSight;
    }

    public bool ShouldTransition(AIInput input)
    {
        if (!input.HasTarget)
            return false;

        float d = Vector2.Distance(input.OurPosition, input.TargetPosition);
        bool close = (this.MaxDistanceForTransition < 0.0f || d <= this.MaxDistanceForTransition) && d >= MinDistanceForTransition;

        if (!close || !this.RequiresLineOfSight)
            return close;

        IntegerVector diff = input.TargetPosition -  new IntegerVector(input.OurPosition.X, input.OurCollider.Bounds.Max.Y);
        return CollisionManager.Instance.RaycastFirst(input.OurPosition, ((Vector2)diff).normalized, ((Vector2)diff).magnitude, 1 << LineOfSightBlocker).Collided == false;
    }
}

public class NoTargetTransition : AIStateTransition
{
    public AIState Destination { get; private set; }

    public NoTargetTransition(AIState destination)
    {
        this.Destination = destination;
    }

    public bool ShouldTransition(AIInput input)
    {
        return !input.HasTarget;
    }
}

public class NoLineOfSightTransition : AIStateTransition
{
    public AIState Destination { get; private set; }

    public NoLineOfSightTransition(AIState destination)
    {
        this.Destination = destination;
    }

    public bool ShouldTransition(AIInput input)
    {
        if (!input.HasTarget)
            return true;

        IntegerVector diff = input.TargetPosition - new IntegerVector(input.OurPosition.X, input.OurCollider.Bounds.Max.Y);
        return CollisionManager.Instance.RaycastFirst(input.OurPosition, ((Vector2)diff).normalized, ((Vector2)diff).magnitude, 1 << TargetWithinRangeTransition.LineOfSightBlocker).Collided;
    }
}

public class TargetChangedTransition : AIStateTransition
{
    public AIState Destination { get; private set; }

    public TargetChangedTransition(AIState destination)
    {
        this.Destination = destination;
    }

    public bool ShouldTransition(AIInput input)
    {
        return input.ChangedTarget;
    }
}

public class TargetAliveTransition : AIStateTransition
{
    public AIState Destination { get; private set; }
    public bool TrueOnAlive { get; private set; }

    public TargetAliveTransition(AIState destination, bool trueOnAlive)
    {
        this.Destination = destination;
        this.TrueOnAlive = trueOnAlive;
    }

    public bool ShouldTransition(AIInput input)
    {
        return this.TrueOnAlive == input.TargetAlive;
    }
}

public class ANDTransition : AIStateTransition
{
    public AIState Destination { get { return this.Transitions[0].Destination; } }
    public AIStateTransition[] Transitions { get; private set; }

    public ANDTransition(AIStateTransition[] transitions)
    {
        this.Transitions = transitions;
    }

    public bool ShouldTransition(AIInput input)
    {
        for (int i = 0; i < this.Transitions.Length; ++i)
        {
            if (!this.Transitions[i].ShouldTransition(input))
                return false;
        }
        return true;
    }
}
