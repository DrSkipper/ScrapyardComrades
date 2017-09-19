using UnityEngine;
using System.Collections.Generic;

public class WindRegion : VoBehavior, IPausable
{
    public LayerMask AffectedMask;
    public Vector2 TargetVelocity;
    public float Acceleration = 3.0f;
    public float Deceleration = 2.0f;

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
    }

    void FixedUpdate()
    {
        _collisions.Clear();
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
                    mod.Behavior = VelocityModifier.CollisionBehavior.sustain;
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
                VelocityModifier mod = actor.GetVelocityModifier(WIND_BOOST);
                mod.Modifier = mod.Modifier.Approach(this.Acceleration, this.TargetVelocity);
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

    /**
     * Private
     */
    private List<GameObject> _collisions;
    private List<Actor2D> _currentAffected;
    private List<Actor2D> _windingDown;
    private const string WIND_BOOST = "WIND";
}
