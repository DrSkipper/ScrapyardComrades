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

    public TargetWithinRangeTransition(AIState destination, float minDistanceForTransition, float maxDistanceForTransition = -1)
    {
        this.Destination = destination;
        this.MinDistanceForTransition = minDistanceForTransition;
        this.MaxDistanceForTransition = maxDistanceForTransition;
    }

    public bool ShouldTransition(AIInput input)
    {
        if (!input.HasTarget)
            return false;

        float d = Vector2.Distance(input.OurPosition, input.TargetPosition);
        return (this.MaxDistanceForTransition < 0.0f || d <= this.MaxDistanceForTransition) && d >= MinDistanceForTransition;
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
