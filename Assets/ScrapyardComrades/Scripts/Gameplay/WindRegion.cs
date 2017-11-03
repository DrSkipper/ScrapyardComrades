using UnityEngine;
using System.Collections.Generic;

public class WindRegion : VoBehavior, IPausable
{
    public LayerMask AffectedMask;
    public Vector2 TargetVelocity;
    public float Acceleration = 3.0f;
    public float Deceleration = 2.0f;
    public bool Activated = true;

    void Awake()
    {
        _collisions = new List<GameObject>();
        _currentAffected = new List<Actor2D>();
        _windingDown = new List<Actor2D>();
    }

    void OnSpawn()
    {
        _currentAffected.Clear();
        _windingDown.Clear();

        GlobalEvents.Notifier.Listen(WorldRecenterEvent.NAME, this, onRecenter);
    }

    void FixedUpdate()
    {
        _collisions.Clear();
        if (this.Activated)
            this.integerCollider.Collide(_collisions, 0, 0, this.AffectedMask);
        
        int i = 0;
        // Check if affected actors have left region
        while (i < _currentAffected.Count)
        {
            Actor2D actor = _currentAffected[i];
            if (actor == null || !_collisions.Contains(actor.gameObject))
            {
                _currentAffected.RemoveAt(i);
                if (actor != null)
                    _windingDown.Add(actor);
            }
            else
            {
                ++i;
            }
        }

        // Add actors that have entered the region
        for (i = 0; i < _collisions.Count; ++i)
        {
            Actor2D actor = _collisions[i].GetComponent<Actor2D>();
            if (actor != null)
            {
                if (!_currentAffected.Contains(actor))
                {
                    _windingDown.Remove(actor); // just in case
                    _currentAffected.Add(actor);

                    VelocityModifier mod = actor.GetVelocityModifier(WIND_BOOST);
                    mod.Behavior = VelocityModifier.CollisionBehavior.nullify;
                    actor.SetVelocityModifier(WIND_BOOST, mod);
                }
            }
        }

        // Accelerate actors within the region
        for (i = 0; i < _currentAffected.Count;)
        {
            Actor2D actor = _currentAffected[i];
            if (actor != null)
            {
                float targetVelocity = this.TargetVelocity.magnitude;

                // If actor is more than halfway through the wind region, interpolate between target velocity and 0 for new target;
                targetVelocity = Mathf.Lerp(targetVelocity, 0, actorPercentInFartherHalfOfRegion(actor));
                
                VelocityModifier mod = actor.GetVelocityModifier(WIND_BOOST);
                float a = mod.Modifier.magnitude > targetVelocity ? this.Deceleration : this.Acceleration;
                mod.Modifier = mod.Modifier.Approach(a, this.TargetVelocity.normalized * targetVelocity);
                ++i;
            }
            else
            {
                _currentAffected.RemoveAt(i);
            }
        }

        // Deccelerate actors that have left the region
        for (i = 0; i < _windingDown.Count;)
        {
            Actor2D actor = _windingDown[i];
            if (actor != null)
            {
                VelocityModifier mod = actor.GetVelocityModifier(WIND_BOOST);
                mod.Modifier = mod.Modifier.Approach(this.Deceleration, Vector2.zero);
                if (mod.Modifier == Vector2.zero)
                {
                    // Remove actors that have finished deccelerating
                    _windingDown.RemoveAt(i);
                    actor.RemoveVelocityModifier(WIND_BOOST);
                }
                else
                {
                    ++i;
                }
            }
            else
            {
                _windingDown.RemoveAt(i);
            }
        }
    }

    void OnReturnToPool()
    {
        GlobalEvents.Notifier.RemoveListenersForOwnerAndEventName(this, WorldRecenterEvent.NAME);
    }

    public void StopTrackingWindingDowns()
    {
        int i;
        while (_windingDown.Count > 0)
        {
            i = _windingDown.Count - 1;
            Actor2D actor = _windingDown[i];
            if (actor != null)
                actor.RemoveVelocityModifier(WIND_BOOST);
            _windingDown.RemoveAt(i);
        }
    }

    /**
     * Private
     */
    private List<GameObject> _collisions;
    private List<Actor2D> _currentAffected;
    private List<Actor2D> _windingDown;
    private const string WIND_BOOST = "WIND";

    private void onRecenter(LocalEventNotifier.Event e)
    {
        this.StopTrackingWindingDowns();
    }

    private float actorPercentInFartherHalfOfRegion(Actor2D actor)
    {
        if (Mathf.Abs(this.integerCollider.Offset.X) > Mathf.Abs(this.integerCollider.Offset.Y))
        {
            if (this.integerCollider.Offset.X > 0)
            {
                // right
                float min = this.transform.position.x + this.integerCollider.Offset.X;
                if (actor.transform.position.x > min)
                {
                    float a = actor.transform.position.x - min;
                    float max = this.integerCollider.Bounds.Size.X / 2;
                    return Mathf.Clamp(a / max, 0, 1);
                }
                return 0;
            }
            else
            {
                // left
                float min = this.transform.position.x + this.integerCollider.Offset.X;
                if (actor.transform.position.x < min)
                {
                    float a = min - actor.transform.position.x;
                    float max = this.integerCollider.Bounds.Size.X / 2;
                    return Mathf.Clamp(a / max, 0, 1);
                }
                return 0;
            }
        }
        else
        {
            if (this.integerCollider.Offset.Y > 0)
            {
                // up
                float min = this.transform.position.y + this.integerCollider.Offset.Y;
                if (actor.transform.position.y > min)
                {
                    float a = actor.transform.position.y - min;
                    float max = this.integerCollider.Bounds.Size.Y / 2;
                    return Mathf.Clamp(a / max, 0, 1);
                }
                return 0;
            }
            else
            {
                // down
                float min = this.transform.position.y + this.integerCollider.Offset.Y;
                if (actor.transform.position.y < min)
                {
                    float a = min - actor.transform.position.y;
                    float max = this.integerCollider.Bounds.Size.Y / 2;
                    return Mathf.Clamp(a / max, 0, 1);
                }
                return 0;
            }
        }
    }
}
