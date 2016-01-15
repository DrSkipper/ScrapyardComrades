using UnityEngine;

public class CollisionEvent : LocalEventNotifier.Event
{
    public static string NAME = "COLLISION";
    public GameObject[] Hits;
    public Vector2 VelocityAtHit; // Velocity of actor at time collision was detected, before being multiplied by Time.deltaTime
    public Vector2 VelocityApplied; // How much of the velocity, AFTER Time.deltaTime multiplier, was applied before detecting the collision

    public CollisionEvent(GameObject[] hits, Vector2 velocity, Vector2 velocityApplied)
    {
        this.Name = NAME;
        this.Hits = hits;
        this.VelocityAtHit = velocity;
        this.VelocityApplied = velocityApplied;
    }
}

public class HitEvent : LocalEventNotifier.Event
{
    public static string NAME = "DAMAGE";
    public GameObject Hit;

    public HitEvent(GameObject hit)
    {
        this.Name = NAME;
        this.Hit = hit;
    }
}

public class PlayerUpdateFinishedEvent : LocalEventNotifier.Event
{
    public static string NAME = "UPDATE_FINISHED";
    public SCAttack CurrentAttack;

    public PlayerUpdateFinishedEvent(SCAttack currentAttack = null)
    {
        this.Name = NAME;
        this.CurrentAttack = currentAttack;
    }
}
