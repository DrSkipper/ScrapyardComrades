using UnityEngine;

public class VelocityModifier
{
    public enum CollisionBehavior
    {
        nullify,
        sustain,
        bounce
    }

    public Vector2 Modifier;
    public CollisionBehavior Behavior;

    public VelocityModifier(Vector2 modifier, CollisionBehavior behavior)
    {
        this.Modifier = modifier;
        this.Behavior = behavior;
    }

    public void CollideX()
    {
        switch (this.Behavior)
        {
            default:
            case CollisionBehavior.sustain:
                break;
            case CollisionBehavior.nullify:
                this.Modifier.x = 0.0f;
                break;
            case CollisionBehavior.bounce:
                this.Modifier.x = -this.Modifier.x;
                break;
        }
    }

    public void CollideY()
    {
        switch (this.Behavior)
        {
            default:
            case CollisionBehavior.sustain:
                break;
            case CollisionBehavior.nullify:
                this.Modifier.y = 0.0f;
                break;
            case CollisionBehavior.bounce:
                this.Modifier.y = -this.Modifier.y;
                break;
        }
    }

    public static VelocityModifier Zero { get { return new VelocityModifier(Vector2.zero, CollisionBehavior.nullify); } }
}
