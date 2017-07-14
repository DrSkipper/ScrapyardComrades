using UnityEngine;

//TODO - Take into account rotation in Overlaps and Collides
public class IntegerRectCollider : IntegerCollider
{
    public IntegerVector Size;
    public override IntegerRect Bounds { get { return new IntegerRect(this.integerPosition + this.Offset, this.Size); } }

    public const int ID = 1;
    public override int Id { get { return ID; } }

    void OnDrawGizmosSelected()
    {
        if (this.enabled)
        {
            IntegerRect bounds = this.Bounds;
            Gizmos.color = this.DebugColor;
            Gizmos.DrawWireCube(new Vector3(bounds.Center.X, bounds.Center.Y), new Vector3(this.Size.X, this.Size.Y));
        }
    }
    
    public override bool Overlaps(IntegerCollider other, int offsetX = 0, int offsetY = 0)
    {
        if (other.Id != ID)
            return other.Overlaps(this, -offsetX, -offsetY);
        return base.Overlaps(other, offsetX, offsetY);
    }
}
