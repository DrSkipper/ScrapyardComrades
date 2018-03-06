using UnityEngine;
using System.Collections.Generic;

public class PatrollingPlatform : VoBehavior, IMovingPlatform, IPausable
{
    public int Speed = 2;
    public Transform Start;
    public Transform Destination;
    public LayerMask BlockingMask;
    public LayerMask ActorMask;

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

    void Awake()
    {
        _collisions = new List<GameObject>();
    }

    void OnSpawn()
    {
        this.integerCollider.AddToCollisionPool();
        this.transform.SetPosition2D(this.Start.position);
        _actualPos = this.transform.position;
        _outward = true;
        _outwardVelocity = (((Vector2)this.Destination.transform.position) - ((Vector2)this.Start.transform.position)).normalized * this.Speed;
        _inwardVelocity = (((Vector2)this.Start.transform.position) - ((Vector2)this.Destination.transform.position)).normalized * this.Speed;

        // Hack check to see if we're not in the level editor
        if (EntityTracker.Instance != null)
        {
            // Disable debug image for our destination
            SpriteRenderer destinationRenderer = this.Destination.GetComponent<SpriteRenderer>();
            if (destinationRenderer != null)
                destinationRenderer.enabled = false;
        }
    }

    void OnReturnToPool()
    {
        this.integerCollider.RemoveFromCollisionPool();
    }

    void FixedUpdate()
    {
        if (!_stopped)
        {
            IntegerVector target = _outward ? (Vector2)this.Destination.transform.position : (Vector2)this.Start.transform.position;
            Vector2 nextActualPos = Vector2.MoveTowards(_actualPos, target, this.Speed);
            IntegerVector nextPos = nextActualPos;
            IntegerVector currentPos = this.integerPosition;
            int offsetX = nextPos.X - currentPos.X;
            int offsetY = nextPos.Y - currentPos.Y;

            // Can we move or are we blocked?
            bool canMove = true;
            this.integerCollider.Collide(_collisions, offsetX, offsetY, this.BlockingMask);

            if (_collisions.Count > 0)
            {
                DirectionalVector2 dir = _outward ? new DirectionalVector2(_outwardVelocity.x, _outwardVelocity.y) : new DirectionalVector2(_inwardVelocity.x, _inwardVelocity.y);
                for (int i = 0; i < _collisions.Count; ++i)
                {
                    // Push actors to make room if possible
                    Actor2D collision = _collisions[i].GetComponent<Actor2D>();
                    if (collision == null || !collision.Push(dir, this.integerCollider, offsetX, offsetY))
                    {
                        canMove = false;
                        break;
                    }
                }
                _collisions.Clear();
            }

            // If we can move, do so, otherwise register that we're blocked here
            if (canMove)
            {
                // If we're moving a negative y value, get a list of the actors on us to pull down with us
                if (offsetY < 0 || offsetX != 0)
                {
                    this.integerCollider.Collide(_collisions, offsetX == 0 ? 0 : -Mathf.RoundToInt(Mathf.Sign(offsetX)), offsetY == 0 ? 0 : 1, this.ActorMask);
                }

                _blocked = false;
                this.transform.SetPosition2D(nextPos);
                _actualPos = nextActualPos;

                if (nextPos == target)
                {
                    //TODO: Stop for a bit
                    _outward = !_outward;
                    _actualPos = nextPos;
                }

                // Pull any actors on our platform down with us
                if ((offsetY < 0 || offsetX != 0) && _collisions.Count > 0)
                {
                    for (int i = 0; i < _collisions.Count; ++i)
                    {
                        _collisions[i].GetComponent<Actor2D>().Move(new Vector2(offsetX, offsetY));
                    }
                    _collisions.Clear();
                }
            }
            else
            {
                _blocked = true;
                _actualPos = currentPos;
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
    private List<GameObject> _collisions;
}
