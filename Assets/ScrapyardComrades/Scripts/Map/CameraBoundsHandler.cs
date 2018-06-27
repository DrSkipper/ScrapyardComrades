
public interface CameraBoundsHandler
{
    IntegerRectCollider GetBounds();
    string CurrentQuadName { get; }
    string PrevQuadName { get; }
}
