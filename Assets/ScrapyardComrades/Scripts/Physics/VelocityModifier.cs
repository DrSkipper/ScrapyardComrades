using UnityEngine;

public class VelocityModifier
{
    [System.Serializable]
    public enum CollisionBehavior
    {
        nullify,
        sustain,
        bounce
    }

    public Vector2 Modifier;
    public CollisionBehavior Behavior;
    public float Parameter;

    public VelocityModifier(Vector2 modifier, CollisionBehavior behavior, float parameter = 1.0f)
    {
        this.Modifier = modifier;
        this.Behavior = behavior;
        this.Parameter = parameter;
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
                this.Modifier.x = -this.Modifier.x * this.Parameter;
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
                this.Modifier.y = -this.Modifier.y * this.Parameter;
                break;
        }
    }

    public static VelocityModifier Zero { get { return new VelocityModifier(Vector2.zero, CollisionBehavior.nullify); } }
}
