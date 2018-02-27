using UnityEngine;

public class PatrollingPlatform : VoBehavior, IMovingPlatform
{
    public int Speed = 2;
    public Transform Start;
    public Transform Destination;
    public LayerMask BlockingMask;

    public Vector2 Velocity
    {
        get
        {
            if (_blocked || _stopped)
                return Vector2.zero;
            else if (_outward)
                return _outwardVelocity;
            else
                return _inwardVelocity;
        }
    }

    void OnSpawn()
    {
        this.transform.SetLocalPosition2D(this.Start.position);
        _actualPos = this.transform.position;
        _outward = true;
        _outwardVelocity = ((Vector2)this.Destination.transform.position) - ((Vector2)this.Start.transform.position).normalized * this.Speed;
        _inwardVelocity = ((Vector2)this.Start.transform.position) - ((Vector2)this.Destination.transform.position).normalized * this.Speed;
    }

    void FixedUpdate()
    {
        IntegerVector target = _outward ? (Vector2)this.Destination.transform.position : (Vector2)this.Start.transform.position;
        Vector2 nextActualPos = Vector2.MoveTowards(_actualPos, target, this.Speed);
        IntegerVector nextPos = nextActualPos;
        IntegerVector currentPos = (Vector2)this.transform.position;

        if (this.integerCollider.CollideFirst(nextPos.X - currentPos.X, nextPos.Y - currentPos.Y, this.BlockingMask) != null)
        {
            this.transform.SetPosition2D(nextPos);
            _actualPos = nextActualPos;

            if (nextPos == (IntegerVector)(Vector2)this.transform.position)
            {
                //TODO: Stop for a bit
                _outward = !_outward;
                _actualPos = nextPos;
            }
        }
    }

    /**
     * Private
     */
    private Vector2 _actualPos;
    private Vector2 _outwardVelocity;
    private Vector2 _inwardVelocity;
    private bool _blocked;
    private bool _stopped;
    private bool _outward;
}
