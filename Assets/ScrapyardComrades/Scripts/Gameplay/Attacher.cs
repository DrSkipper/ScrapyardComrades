using UnityEngine;

public class Attacher : VoBehavior, IPausable
{
    public IntegerRectCollider Collider;
    public TurretController.AttachDir AttachedAt = TurretController.AttachDir.Down;
    public bool AttachToSurfaces = true;
    public LayerMask SurfaceLayers;

    void Awake()
    {
        _colliderOffset = this.Collider.Offset;
        _colliderSize = this.Collider.Size;
    }

    void OnSpawn()
    {
        _hasAttached = false;
        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                this.Collider.Offset = _colliderOffset;
                this.Collider.Size = _colliderSize;
                break;
            case TurretController.AttachDir.Up:
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                this.Collider.Offset = new IntegerVector(-_colliderOffset.X, -_colliderOffset.Y);
                this.Collider.Size = _colliderSize;
                break;
            case TurretController.AttachDir.Left:
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                this.Collider.Offset = new IntegerVector(_colliderOffset.Y, -_colliderOffset.X);
                this.Collider.Size = new IntegerVector(_colliderSize.Y, _colliderSize.X);
                break;
            case TurretController.AttachDir.Right:
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                this.Collider.Offset = new IntegerVector(-_colliderOffset.Y, _colliderOffset.X);
                this.Collider.Size = new IntegerVector(_colliderSize.Y, _colliderSize.X);
                break;
        }
    }

    void FixedUpdate()
    {
        if (!_hasAttached)
            attachToSurface();
    }

    /**
     * Private
     */
    private bool _hasAttached;
    private IntegerVector _colliderOffset;
    private IntegerVector _colliderSize;
    
    private void attachToSurface()
    {
        _hasAttached = true;
        int offsetX = 0;
        int offsetY = 0;

        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                offsetY = -2;
                break;
            case TurretController.AttachDir.Left:
                offsetX = -2;
                break;
            case TurretController.AttachDir.Up:
                offsetY = 2;
                break;
            case TurretController.AttachDir.Right:
                offsetX = 2;
                break;
        }

        GameObject surface = this.integerCollider.CollideFirst(offsetX, offsetY, this.SurfaceLayers);
        if (surface != null)
        {
            this.transform.SetParent(surface.transform);
        }
    }
}
