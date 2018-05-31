using UnityEngine;

public class MineLauncher : MonoBehaviour, IPausable
{
    public PooledObject MinePrefab;
    public int LaunchInterval = 50;
    public float LaunchSpeed = 3.0f;
    public TurretController.AttachDir AttachedAt = TurretController.AttachDir.Left;
    public TurretController.AttachDir MineAttachDir = TurretController.AttachDir.Down;
    public Transform SpawnLocation;
    public bool IsLaunching;
    public bool AttachToSurfaces = true;
    public LayerMask SurfaceLayers;

    void OnSpawn()
    {
        _hasAttached = !this.AttachToSurfaces;
        if (_spawnTimer == null)
            _spawnTimer = new Timer(this.LaunchInterval, true, true, onLaunch);
        else
            _spawnTimer.resetAndStart(this.LaunchInterval);
        
        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Left:
                this.transform.rotation = Quaternion.identity;
                this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                _launchVelocity = new Vector2(this.LaunchSpeed, 0.0f);
                break;
            case TurretController.AttachDir.Right:
                this.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                this.transform.rotation = Quaternion.identity;
                _launchVelocity = new Vector2(-this.LaunchSpeed, 0.0f);
                break;
            case TurretController.AttachDir.Up:
                this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                _launchVelocity = new Vector2(0.0f, -this.LaunchSpeed);
                break;
            case TurretController.AttachDir.Down:
                this.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                this.transform.rotation = Quaternion.Euler(0, 0, -90);
                _launchVelocity = new Vector2(0.0f, this.LaunchSpeed);
                break;
        }

        if (!this.IsLaunching)
        {
            _spawnTimer.Paused = true;
        }
    }

    void FixedUpdate()
    {
        if (!_hasAttached)
            attachToSurface();
        _spawnTimer.update();
    }

    /**
     * Private
     */
    private Timer _spawnTimer;
    private Vector2 _launchVelocity;
    private bool _hasAttached;

    private void onLaunch()
    {
        PooledObject mine = this.MinePrefab.Retain();
        mine.GetComponent<Mine>().AttachedAt = this.MineAttachDir;
        mine.GetComponent<Actor2D>().Velocity = _launchVelocity;
        mine.transform.position = this.SpawnLocation.position;
        mine.BroadcastMessage(ObjectPlacer.ON_SPAWN_METHOD, SendMessageOptions.DontRequireReceiver);
    }

    private void attachToSurface()
    {
        _hasAttached = true;
        int offsetX = 0;
        int offsetY = 0;

        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                offsetY = -4;
                break;
            case TurretController.AttachDir.Left:
                offsetX = -4;
                break;
            case TurretController.AttachDir.Up:
                offsetY = 4;
                break;
            case TurretController.AttachDir.Right:
                offsetX = 4;
                break;
        }

        GameObject surface = CollisionManager.Instance.CollidePointFirst(new IntegerVector(Mathf.RoundToInt(this.transform.position.x) + offsetX, Mathf.RoundToInt(this.transform.position.y) + offsetY), this.SurfaceLayers);
        if (surface != null)
        {
            this.transform.SetParent(surface.transform);
            this.transform.SetLocalZ(-1);
        }
    }
}
