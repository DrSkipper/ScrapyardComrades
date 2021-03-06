﻿using UnityEngine;
using System.Collections.Generic;

public abstract class IntegerCollider : VoBehavior
{
    public bool AddOnStart = true;
    public IntegerVector Offset = IntegerVector.Zero;
    public abstract IntegerRect Bounds { get; }
    public Color DebugColor = Color.red;

    public abstract int Id { get; }

    void Start()
    {
        if (this.AddOnStart)
            this.AddToCollisionPool();
    }

    public override void OnDestroy()
    {
        if (this.CollisionManager != null)
            this.CollisionManager.RemoveCollider(this.layerMask, this);
        base.OnDestroy();
    }

    public void AddToCollisionPool()
    {
        if (this.CollisionManager != null)
            this.CollisionManager.AddCollider(this.layerMask, this);
    }

    public void RemoveFromCollisionPool()
    {
        if (this.CollisionManager != null)
            this.CollisionManager.RemoveCollider(this.layerMask, this);
    }

    //TODO - use dynamic keyword to make use of run-time overloading? and have an extensions method place for inter-collider type collisions?
    //http://stackoverflow.com/questions/13095544/overloaded-method-why-is-base-class-given-precedence#comment25529590_13096565
    public virtual bool Overlaps(IntegerCollider other, int offsetX = 0, int offsetY = 0)
    {
        IntegerRect bounds = this.Bounds;
        bounds.Center.X += offsetX;
        bounds.Center.Y += offsetY;
        return bounds.Overlaps(other.Bounds);
    }

    public virtual bool Contains(IntegerVector point, int offsetX = 0, int offsetY = 0)
    {
        IntegerRect bounds = this.Bounds;
        bounds.Center.X += offsetX;
        bounds.Center.Y += offsetY;
        return bounds.Contains(point);
    }

    public virtual IntegerVector ClosestContainedPoint(IntegerVector point)
    {
        return this.Bounds.ClosestContainedPoint(point);
    }

    public GameObject CollideFirst(int offsetX = 0, int offsetY = 0, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null, List<IntegerCollider> potentialCollisions = null)
    {
        if (potentialCollisions == null)
            potentialCollisions = this.GetPotentialCollisions(0, 0, offsetX, offsetY, mask);

        if (potentialCollisions.Count == 0 || (potentialCollisions.Count == 1 && potentialCollisions[0] == this))
            return null;
        
        for (int i = 0; i < potentialCollisions.Count; ++i)
        {
            IntegerCollider collider = potentialCollisions[i];
            if (collider != null && collider != this && (objectTag == null || collider.tag == objectTag) && collider.enabled)
            {
                if (this.Overlaps(collider, offsetX, offsetY))
                    return collider.gameObject;
            }
        }

        return null;
    }

    public void Collide(List<GameObject> collisions, int offsetX = 0, int offsetY = 0, int mask = Physics2D.DefaultRaycastLayers, string objectTag = null, List<IntegerCollider> potentialCollisions = null)
    {
        if (potentialCollisions == null)
            potentialCollisions = this.GetPotentialCollisions(0, 0, offsetX, offsetY, mask);

        if (potentialCollisions.Count == 0 || (potentialCollisions.Count == 1 && potentialCollisions[0] == this))
            return;
        
        for (int i = 0; i < potentialCollisions.Count; ++i)
        {
            IntegerCollider collider = potentialCollisions[i];
            if (collider != null && collider != this && (objectTag == null || collider.tag == objectTag) && collider.enabled)
            {
                if (this.Overlaps(collider, offsetX, offsetY))
                    collisions.AddUnique(collider.gameObject);
            }
        }
    }

    public List<IntegerCollider> GetPotentialCollisions(float vx, float vy, int offsetX = 0, int offsetY = 0, int mask = Physics2D.DefaultRaycastLayers, List<IntegerCollider> _listRef = null, int enlargeX = 0, int enlargeY = 0)
    {
        if (this.CollisionManager == null)
            return new List<IntegerCollider>();

        IntegerRect bounds = this.Bounds;
        IntegerRect range = new IntegerRect(bounds.Center.X + offsetX, bounds.Center.Y + offsetY, this.Bounds.Size.X + Mathf.RoundToInt(Mathf.Abs(vx * 2) + 1.55f) + enlargeX, this.Bounds.Size.Y + (Mathf.RoundToInt(Mathf.Abs(vy * 2) + 1.55f)) + enlargeY);
        return this.CollisionManager.GetCollidersInRange(range, mask, null, _listRef);
    }

    public bool CollideCheck(GameObject checkObject, int offsetX = 0, int offsetY = 0)
    {
        IntegerCollider other = checkObject.GetComponent<IntegerCollider>();
        return other && this.Overlaps(other, offsetX, offsetY);
    }
}
