using UnityEngine;
using System;
using System.Collections.Generic;

public class Actor2D : VoBehavior
{
    public Vector2 Velocity;
    public LayerMask HaltMovementMask;
    public LayerMask CollisionMask;
    public bool CheckCollisionsWhenStill = false;
    public Transform ActualPosition;

    public const float MAX_POSITION_INCREMENT = 1.0f;
    public const int BOUNCE_DETECTION_RANGE = 1;

    public virtual void Update()
    {
        Vector2 modifiedVelocity = this.Velocity;
        foreach (VelocityModifier modifier in _velocityModifiers.Values)
            modifiedVelocity += modifier.Modifier;

        if (modifiedVelocity.x != 0.0f || modifiedVelocity.y != 0.0f)
        {
            Move(modifiedVelocity * Time.deltaTime);
        }
        else if (this.CheckCollisionsWhenStill)
        {
            this.integerCollider.Collide(_collisionsFromMove, 0, 0, this.CollisionMask);

            if (_collisionsFromMove.Count > 0)
            {
                GameObject[] collisions = _collisionsFromMove.ToArray();
                _collisionsFromMove.Clear();
                this.localNotifier.SendEvent(new CollisionEvent(collisions, Vector2.zero, Vector2.zero));

                foreach (GameObject collision in collisions)
                    this.localNotifier.SendEvent(new HitEvent(collision));
            }
        }

        if (this.ActualPosition != null)
            this.ActualPosition.localPosition = new Vector3(_positionModifier.x, _positionModifier.y);
    }

    public void Move(Vector2 d)
    {
        float incX = d.x;
        float incY = d.y;

        List<IntegerCollider> potentialCollisions = null;

        if (Mathf.Abs(incX) > MAX_POSITION_INCREMENT || Mathf.Abs(incY) > MAX_POSITION_INCREMENT)
        {
            Vector2 dNormalized = d.normalized * MAX_POSITION_INCREMENT;
            incX = dNormalized.x;
            incY = dNormalized.y;

            potentialCollisions = this.integerCollider.GetPotentialCollisions(d.x, d.y, 0, 0, this.CollisionMask);
        }

        Vector2 soFar = Vector2.zero;
        Vector2 projected = Vector2.zero;
        Vector2 oldVelocity = this.Velocity;
        float dMagnitude = d.magnitude;

        while (true)
        {
            projected.x += incX;
            projected.y += incY;

            if (projected.magnitude > dMagnitude)
            {
                if (!_haltX) moveX(d.x - soFar.x, _collisionsFromMove, _horizontalCollisions, potentialCollisions);
                if (!_haltY) moveY(d.y - soFar.y, _collisionsFromMove, _verticalCollisions, potentialCollisions);
                soFar = d;
                break;
            }

            if (!_haltX) soFar.x += moveX(incX, _collisionsFromMove, _horizontalCollisions, potentialCollisions);
            if (!_haltY) soFar.y += moveY(incY, _collisionsFromMove, _verticalCollisions, potentialCollisions);

            if (_haltX && _haltY)
                break;
            
            if (soFar.magnitude >= dMagnitude)
                break;
        }

        if (_haltX)
        {
            this.Velocity.x = 0;
            foreach (VelocityModifier modifier in _velocityModifiers.Values)
                modifier.CollideX();
        }
        if (_haltY)
        {
            this.Velocity.y = 0;
            foreach (VelocityModifier modifier in _velocityModifiers.Values)
                modifier.CollideY();
        }

        if (_collisionsFromMove.Count > 0)
        {
            GameObject[] collisions = _collisionsFromMove.ToArray();
            _haltX = false;
            _haltY = false;
            _collisionsFromMove.Clear();
            _horizontalCollisions.Clear();
            _verticalCollisions.Clear();
            this.localNotifier.SendEvent(new CollisionEvent(collisions, oldVelocity, soFar));

            foreach (GameObject collision in collisions)
                this.localNotifier.SendEvent(new HitEvent(collision));
        }
    }

    public void SetVelocityModifier(string key, VelocityModifier v)
    {
        if (_velocityModifiers.ContainsKey(key))
            _velocityModifiers[key] = v;
        else
            _velocityModifiers.Add(key, v);
    }

    public VelocityModifier GetVelocityModifier(string key)
    {
        if (_velocityModifiers.ContainsKey(key))
            return _velocityModifiers[key];
        return VelocityModifier.Zero;
    }

    public void RemoveVelocityModifier(string key)
    {
        _velocityModifiers.Remove(key);
    }


    public bool Bounce(GameObject hit, Vector2 origVelocity, Vector2 appliedVelocity, LayerMask bounceLayerMask, float minimumBounceAngle)
    {
        this.Velocity = origVelocity;
        float remainingSpeed = (origVelocity * Time.deltaTime - appliedVelocity).magnitude;

        int unitDirX = Math.Sign(origVelocity.x) * BOUNCE_DETECTION_RANGE;
        int unitDirY = Math.Sign(origVelocity.y) * BOUNCE_DETECTION_RANGE;

        bool verticalPlane = unitDirX != 0 && this.integerCollider.CollideFirst(unitDirX, 0, bounceLayerMask) != null;
        bool horizontalPlane = unitDirY != 0 && this.integerCollider.CollideFirst(0, unitDirY, bounceLayerMask) != null;

        if (verticalPlane)
            this.Velocity.x = -this.Velocity.x;

        if (horizontalPlane)
            this.Velocity.y = -this.Velocity.y;

        // Only continue the bounce if our angle is within bounce range
        if (Mathf.Abs(180.0f - Vector2.Angle(origVelocity, this.Velocity)) < minimumBounceAngle)
        {
            this.Velocity = Vector2.zero;
            return false;
        }

        this.Move(this.Velocity.normalized * remainingSpeed);
        return true;
    }

    /**
     * Private
     */
    private Vector2 _positionModifier = Vector2.zero;
    private List<GameObject> _collisionsFromMove = new List<GameObject>();
    private List<GameObject> _horizontalCollisions = new List<GameObject>();
    private List<GameObject> _verticalCollisions = new List<GameObject>();
    private Dictionary<string, VelocityModifier> _velocityModifiers = new Dictionary<string, VelocityModifier>();
    private bool _haltX;
    private bool _haltY;
    
    // Returns actual amount applied to movement
    private float moveX(float dx, List<GameObject> collisions, List<GameObject> horizontalCollisions, List<IntegerCollider> potentialCollisions)
    {
        _positionModifier.x += dx;
        int unitMove = Mathf.RoundToInt(_positionModifier.x);

        if (unitMove != 0)
        {
            int moves = 0;
            int unitDir = Math.Sign(unitMove);
            _positionModifier.x -= unitMove;

            while (unitMove != 0)
            {
                int oldCount = horizontalCollisions.Count;
                this.integerCollider.Collide(horizontalCollisions, unitDir, 0, this.CollisionMask, null, potentialCollisions);

                if (horizontalCollisions.Count > oldCount)
                {
                    for (int i = oldCount; i < horizontalCollisions.Count; ++i)
                    {
                        collisions.AddUnique(horizontalCollisions[i]);

                        if (((1 << horizontalCollisions[i].layer) & this.HaltMovementMask) != 0)
                        {
                            _positionModifier.x = 0.0f;
                            _haltX = true;
                            return moves;
                        }
                    }
                }

                this.transform.position += new Vector3(unitDir, 0, 0);
                unitMove -= unitDir;
                ++moves;
            }
        }

        return dx;
    }
    
    private float moveY(float dy, List<GameObject> collisions, List<GameObject> verticalCollisions, List<IntegerCollider> potentialCollisions)
    {
        _positionModifier.y += dy;
        int unitMove = Mathf.RoundToInt(_positionModifier.y);

        if (unitMove != 0)
        {
            int moves = 0;
            int unitDir = Math.Sign(unitMove);
            _positionModifier.y -= unitMove;

            while (unitMove != 0)
            {
                int oldCount = verticalCollisions.Count;
                this.integerCollider.Collide(verticalCollisions, 0, unitDir, this.CollisionMask, null, potentialCollisions);

                if (verticalCollisions.Count > oldCount)
                {
                    for (int i = oldCount; i < verticalCollisions.Count; ++i)
                    {
                        collisions.AddUnique(verticalCollisions[i]);

                        if (((1 << verticalCollisions[i].layer) & this.HaltMovementMask) != 0)
                        {
                            _positionModifier.y = 0.0f;
                            _haltY = true;
                            return moves;
                        }
                    }
                }

                this.transform.position += new Vector3(0, unitDir, 0);
                unitMove -= unitDir;
                ++moves;
            }
        }

        return dy;
    }
}
