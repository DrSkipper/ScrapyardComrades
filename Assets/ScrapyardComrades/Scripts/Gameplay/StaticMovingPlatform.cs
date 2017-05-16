using UnityEngine;

public class StaticMovingPlatform : VoBehavior, IMovingPlatform
{
    public Vector2 StaticVelociy;
    public Vector2 Velocity { get { return this.StaticVelociy; } }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
    }
    
    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }
}
