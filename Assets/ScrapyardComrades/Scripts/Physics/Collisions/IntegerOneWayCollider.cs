using UnityEngine;

// Assumes vertical and up facing for now
public class IntegerOneWayCollider : IntegerCollider
{
    public int Width = 10;
    public override IntegerRect Bounds { get { return new IntegerRect(this.integerPosition + this.Offset, new IntegerVector(this.Width, 0)); } }

    public const int ID = 3;
    public override int Id { get { return ID; } }

    void OnDrawGizmosSelected()
    {
        if (this.enabled)
        {
            IntegerRect bounds = this.Bounds;
            Gizmos.color = this.DebugColor;
            Gizmos.DrawLine((Vector2)bounds.Min, (Vector2)bounds.Max);
        }
    }

    //NOTE: Only partial support of circle colliders here
    public override bool Overlaps(IntegerCollider other, int offsetX = 0, int offsetY = 0)
    {
        IntegerRect otherBounds = other.Bounds;
        int otherY = other.Bounds.Min.Y;
        IntegerVector ourPos = this.integerPosition + this.Offset;

        // Check if the colliders current position is above us, and their new (offset) position is on or below us
        if (otherY > ourPos.Y && otherY - offsetY <= ourPos.Y)
        {
            IntegerRect ourBounds = this.Bounds;

            // Make sure they're within our width
            return otherBounds.Min.X - offsetX <= ourBounds.Max.X && otherBounds.Max.X - offsetX >= ourBounds.Min.X;
        }
        return false;
    }
}
