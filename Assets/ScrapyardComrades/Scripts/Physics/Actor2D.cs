using UnityEngine;
using System;
using System.Collections.Generic;

public class Actor2D : VoBehavior, IPausable
{
    public Vector2 Velocity;
    public LayerMask HaltMovementMask;
    public LayerMask CollisionMask;
    public bool CheckCollisionsWhenStill = false;
    public Transform ActualPosition;
    public int BonkGrace = 0;
    public float MinVForExclusiveBonkGrace = 1.0f;

    public const float MAX_POSITION_INCREMENT = 1.0f;
    public const int BOUNCE_DETECTION_RANGE = 1;

    public Vector2 TotalVelocity
    {
        get
        {
            Vector2 modifiedVelocity = this.Velocity;
            foreach (VelocityModifier modifier in _velocityModifiers.Values)
                modifiedVelocity += modifier.Modifier;
            return modifiedVelocity;
        }
    }

    public virtual void FixedUpdate()
    {
        /*_tempCollisions.Clear();
        if (checkedBlocked(_tempCollisions, 0, 0, null))
        {
            Debug.LogWarning("Moved into invalid position!");
        }*/

        Vector2 modifiedVelocity = this.TotalVelocity;
        if (modifiedVelocity.x != 0.0f || modifiedVelocity.y != 0.0f)
        {
            Move(modifiedVelocity);
        }
        else if (this.CheckCollisionsWhenStill)
        {
            this.integerCollider.Collide(_collisionsFromMove, 0, 0, this.CollisionMask);

            if (_collisionsFromMove.Count > 0)
            {
                if (_collisionEvent == null)
                {
                    _collisionEvent = new CollisionEvent(_collisionsFromMove, Vector2.zero, Vector2.zero, false, false, true);
                }
                else
                {
                    _collisionEvent.VelocityAtHit = Vector2.zero;
                    _collisionEvent.VelocityApplied = Vector2.zero;
                    _collisionEvent.CollideX = false;
                    _collisionEvent.CollideY = false;
                    _collisionEvent.MovementHalted = true;
                }
                this.localNotifier.SendEvent(_collisionEvent);
                _collisionsFromMove.Clear();
            }
        }

        if (this.ActualPosition != null)
            this.ActualPosition.localPosition = new Vector3(_positionModifier.x, _positionModifier.y);
    }

    public void Move(Vector2 d, List<IntegerCollider> potentialCollisions = null)
    {
        if (this.integerCollider == null)
        {
            // Non collision check movement
            _positionModifier += d;
            IntegerVector integerPos = this.integerPosition;
            int unitX = Mathf.RoundToInt(_positionModifier.x);
            int unitY = Mathf.RoundToInt(_positionModifier.y);
            _positionModifier.x -= unitX;
            _positionModifier.y -= unitY;
            this.transform.SetPosition2D(integerPos.X + unitX, integerPos.Y + unitY);
            return;
        }

        float incX = d.x;
        float incY = d.y;

        if (potentialCollisions == null)
            potentialCollisions = this.integerCollider.GetPotentialCollisions(d.x, d.y, 0, 0, this.CollisionMask, _potentialCollisionsCache, this.BonkGrace, this.BonkGrace);

        if (Mathf.Abs(incX) > MAX_POSITION_INCREMENT || Mathf.Abs(incY) > MAX_POSITION_INCREMENT)
        {
            Vector2 dNormalized = d.normalized * MAX_POSITION_INCREMENT;
            incX = dNormalized.x;
            incY = dNormalized.y;
        }

        Vector2 soFar = Vector2.zero;
        Vector2 projected = Vector2.zero;
        Vector2 oldVelocity = this.Velocity;
        float dMagnitude = d.magnitude;
        int collisionCount = 0;
        bool collideX = false;
        bool collideY = false;

        while (true)
        {
            projected.x += incX;
            projected.y += incY;

            if (projected.magnitude > dMagnitude)
            {
                if (!_haltX)
                {
                    moveX(d.x - soFar.x, _collisionsFromMove, _horizontalCollisions, potentialCollisions, d.y);

                    if (_collisionsFromMove.Count > collisionCount)
                        collideX = true;
                    collisionCount = _collisionsFromMove.Count;
                }
                if (!_haltY)
                {
                    moveY(d.y - soFar.y, _collisionsFromMove, _verticalCollisions, potentialCollisions, d.x);

                    if (_collisionsFromMove.Count > collisionCount)
                        collideY = true;
                }

                collisionCount = _collisionsFromMove.Count;
                soFar = d;
                break;
            }

            if (!_haltX)
            {
                soFar.x += moveX(incX, _collisionsFromMove, _horizontalCollisions, potentialCollisions, d.y);

                if (_collisionsFromMove.Count > collisionCount)
                    collideX = true;
                collisionCount = _collisionsFromMove.Count;
            }

            if (!_haltY)
            {
                soFar.y += moveY(incY, _collisionsFromMove, _verticalCollisions, potentialCollisions, d.x);

                if (_collisionsFromMove.Count > collisionCount)
                    collideY = true;
            }

            collisionCount = _collisionsFromMove.Count;

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
            _horizontalCollisions.Clear();
            _verticalCollisions.Clear();
            if (_collisionEvent == null)
            {
                _collisionEvent = new CollisionEvent(_collisionsFromMove, oldVelocity, soFar, collideX, collideY, _haltX || _haltY);
            }
            else
            {
                _collisionEvent.VelocityAtHit = oldVelocity;
                _collisionEvent.VelocityApplied = soFar;
                _collisionEvent.CollideX = collideX;
                _collisionEvent.CollideY = collideY;
                _collisionEvent.MovementHalted = _haltX || _haltY;
            }

            _haltX = false;
            _haltY = false;
            this.localNotifier.SendEvent(_collisionEvent);
            _collisionsFromMove.Clear();
        }

        /*_tempCollisions.Clear();
        if (checkedBlocked(_tempCollisions, 0, 0, null))
        {
            Debug.LogWarning("Moved into invalid position!");
        }*/
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
        float remainingSpeed = (origVelocity - appliedVelocity).magnitude;

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

    public bool Push(DirectionalVector2 dir, IntegerCollider fromBounds, int fromOffsetX, int fromOffsetY)
    {
        IntegerVector d = IntegerVector.Zero;
        while (fromBounds.CollideCheck(this.gameObject, fromOffsetX - d.X, fromOffsetY - d.Y))
        {
            d.X += dir.X;
            d.Y += dir.Y;
        }

        IntegerVector target = this.integerPosition + d;
        this.Move(d);

        return this.integerPosition == target;
    }

    /**
     * Private
     */
    private Vector2 _positionModifier = Vector2.zero;
    private List<GameObject> _collisionsFromMove = new List<GameObject>();
    private List<GameObject> _horizontalCollisions = new List<GameObject>();
    private List<GameObject> _verticalCollisions = new List<GameObject>();
    private List<GameObject> _tempCollisions = new List<GameObject>();
    private List<IntegerCollider> _potentialCollisionsCache = new List<IntegerCollider>();
    private Dictionary<string, VelocityModifier> _velocityModifiers = new Dictionary<string, VelocityModifier>();
    private bool _haltX;
    private bool _haltY;
    private CollisionEvent _collisionEvent;

    // Returns actual amount applied to movement (along x axis)
    private float moveX(float dx, List<GameObject> collisions, List<GameObject> horizontalCollisions, List<IntegerCollider> potentialCollisions, float totalDY)
    {
        _positionModifier.x += dx;
        int unitMove = Mathf.RoundToInt(_positionModifier.x);

        // Check if we've moved far enough virtually to actually increment our position by an integer unit
        if (unitMove != 0)
        {
            int moves = 0;
            int unitDir = Math.Sign(unitMove);
            _positionModifier.x -= unitMove;

            while (unitMove != 0)
            {
                int bonkOffset = 0;
                int oldCount = horizontalCollisions.Count;
                bool blocked = checkedBlocked(horizontalCollisions, unitDir, 0, potentialCollisions);

                // If we're blocked, try to bonk grace ourselves around the corner
                while (blocked && Mathf.Abs(bonkOffset) < this.BonkGrace)
                {
                    bonkOffset = Mathf.Abs(bonkOffset);
                    bonkOffset += 1;
                    if (totalDY > -this.MinVForExclusiveBonkGrace)
                    {
                        _tempCollisions.Clear();
                        blocked = checkedBlocked(_tempCollisions, unitDir, bonkOffset, potentialCollisions);
                    }

                    if (blocked && totalDY < this.MinVForExclusiveBonkGrace)
                    {
                        bonkOffset = -bonkOffset;
                        _tempCollisions.Clear();
                        blocked = checkedBlocked(_tempCollisions, unitDir, bonkOffset, potentialCollisions);
                    }
                    
                    if (!blocked)
                    {
                        horizontalCollisions.RemoveRange(oldCount, horizontalCollisions.Count - oldCount);
                        horizontalCollisions.AddUnique(_tempCollisions);
                    }
                }

                if (horizontalCollisions.Count > oldCount)
                    collisions.AddUnique(horizontalCollisions, oldCount);

                // If we're still blocked after attempting bonk grace, halt movement in this axis
                if (blocked)
                {
                    _positionModifier.x = 0.0f;
                    _haltX = true;
                    return moves;
                }

                // Otherwise we're good, so increment our position
                this.transform.SetPosition2D(Mathf.Round(this.transform.position.x + unitDir), Mathf.Round(this.transform.position.y + bonkOffset));
                unitMove -= unitDir;
                ++moves;

                /*_tempCollisions.Clear();
                if (checkedBlocked(_tempCollisions, 0, 0, null))
                {
                    Debug.LogWarning("Moved into invalid position! along x axis");
                }*/
            }
        }

        return dx;
    }

    // Returns actual amount applied to movement (along y axis)
    private float moveY(float dy, List<GameObject> collisions, List<GameObject> verticalCollisions, List<IntegerCollider> potentialCollisions, float totalDX)
    {
        _positionModifier.y += dy;
        int unitMove = Mathf.RoundToInt(_positionModifier.y);

        // Check if we've moved far enough virtually to actually increment our position by an integer unit
        if (unitMove != 0)
        {
            int moves = 0;
            int unitDir = Math.Sign(unitMove);
            _positionModifier.y -= unitMove;

            while (unitMove != 0)
            {
                int bonkOffset = 0;
                int oldCount = verticalCollisions.Count;
                bool blocked = checkedBlocked(verticalCollisions, 0, unitDir, potentialCollisions);

                // If we're blocked, try to bonk grace ourselves around the corner
                while (blocked && Mathf.Abs(bonkOffset) < this.BonkGrace)
                {
                    bonkOffset = Mathf.Abs(bonkOffset);
                    bonkOffset += 1;

                    if (totalDX > -this.MinVForExclusiveBonkGrace)
                    {
                        _tempCollisions.Clear();
                        blocked = checkedBlocked(_tempCollisions, bonkOffset, unitDir, potentialCollisions);
                    }

                    if (blocked && totalDX < this.MinVForExclusiveBonkGrace)
                    {
                        bonkOffset = -bonkOffset;
                        _tempCollisions.Clear();
                        blocked = checkedBlocked(_tempCollisions, bonkOffset, unitDir, potentialCollisions);
                    }

                    if (!blocked)
                    {
                        verticalCollisions.RemoveRange(oldCount, verticalCollisions.Count - oldCount);
                        verticalCollisions.AddUnique(_tempCollisions);
                    }
                }
                
                if (verticalCollisions.Count > oldCount)
                    collisions.AddUnique(verticalCollisions, oldCount);

                // If we're still blocked after attempting bonk grace, halt movement in this axis
                if (blocked)
                {
                    _positionModifier.y = 0.0f;
                    _haltY = true;
                    return moves;
                }

                // Otherwise we're good, so increment our position
                this.transform.SetPosition2D(Mathf.Round(this.transform.position.x + bonkOffset), Mathf.Round(this.transform.position.y + unitDir));
                unitMove -= unitDir;
                ++moves;

                /*_tempCollisions.Clear();
                if (checkedBlocked(_tempCollisions, 0, 0, null))
                {
                    Debug.LogWarning("Moved into invalid position! along y axis");
                }*/
            }
        }

        return dy;
    }

    private bool checkedBlocked(List<GameObject> collisions, int xOffset, int yOffset, List<IntegerCollider> potentialCollisions)
    {
        int oldCount = collisions.Count;
        this.integerCollider.Collide(collisions, xOffset, yOffset, this.CollisionMask, null, potentialCollisions);
        
        for (int i = oldCount; i < collisions.Count; ++i)
        {
            if (this.HaltMovementMask.ContainsLayer(collisions[i].layer))
                return true;
        }
        return false;
    }
}
