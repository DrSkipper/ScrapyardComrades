using UnityEngine;

public class PatrollingPlatform : VoBehavior, IMovingPlatform
{
    public int Speed = 2;
    public Transform Start;
    public Transform Destination;
    public Vector2 Velocity { get { return this.Actor.Velocity; } }

    void OnSpawn()
    {
        
    }

    //public
}
