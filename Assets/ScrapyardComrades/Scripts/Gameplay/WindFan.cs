using UnityEngine;

public class WindFan : MonoBehaviour
{
    public Transform SpriteTransform;
    public WindRegion WindRegionScript;
    public IntegerRectCollider WindRegionCollider;
    public IntegerRectCollider ParticleSpawnRect;
    public MoveAndFadeParticleEmitter Particles;
    public SwitchListener SwitchListener;

    public TurretController.AttachDir AttachedAt = TurretController.AttachDir.Down;

    void Awake()
    {
        _defaultRegionOffset = this.WindRegionCollider.Offset;
        _defaultRegionSize = this.WindRegionCollider.Size;
        _defaultParticleSpawnOffset = this.ParticleSpawnRect.Offset;
        _defaultParticleSpawnSize = this.ParticleSpawnRect.Size;
        _defaultParticleVelocity = this.Particles.PooledParticles[0].GetComponent<MoveAndFadeParticle>().VelocityDir;
        _defaultTargetVelocity = this.WindRegionScript.TargetVelocity;

        if (this.SwitchListener != null)
            this.SwitchListener.StateChangeCallback += onSwitchStateChange;
    }

    void OnSpawn()
    {
        IntegerVector particleVelocityDir;
        switch (this.AttachedAt)
        {
            default:
            case TurretController.AttachDir.Down:
                this.SpriteTransform.rotation = Quaternion.Euler(0, 0, 0);
                this.WindRegionCollider.Offset = _defaultRegionOffset;
                this.WindRegionCollider.Size = _defaultRegionSize;
                this.ParticleSpawnRect.Offset = _defaultParticleSpawnOffset;
                this.ParticleSpawnRect.Size = _defaultParticleSpawnSize;
                particleVelocityDir = _defaultParticleVelocity;
                this.WindRegionScript.TargetVelocity = _defaultTargetVelocity;
                break;
            case TurretController.AttachDir.Up:
                this.SpriteTransform.rotation = Quaternion.Euler(0, 0, 180);
                this.WindRegionCollider.Offset = new IntegerVector(-_defaultRegionOffset.X, -_defaultRegionOffset.Y);
                this.WindRegionCollider.Size = _defaultRegionSize;
                this.ParticleSpawnRect.Offset = new IntegerVector(-_defaultParticleSpawnOffset.X, -_defaultParticleSpawnOffset.Y);
                this.ParticleSpawnRect.Size = _defaultParticleSpawnSize;
                particleVelocityDir = new IntegerVector(-_defaultParticleVelocity.X, -_defaultParticleVelocity.Y);
                this.WindRegionScript.TargetVelocity = new Vector2(-_defaultTargetVelocity.x, -_defaultTargetVelocity.y);
                break;
            case TurretController.AttachDir.Left:
                this.SpriteTransform.rotation = Quaternion.Euler(0, 0, -90);
                this.WindRegionCollider.Offset = new IntegerVector(_defaultRegionOffset.Y, _defaultRegionOffset.X);
                this.WindRegionCollider.Size = new IntegerVector(_defaultRegionSize.Y, _defaultRegionSize.X);
                this.ParticleSpawnRect.Offset = new IntegerVector(_defaultParticleSpawnOffset.Y, _defaultParticleSpawnOffset.X);
                this.ParticleSpawnRect.Size = new IntegerVector(_defaultParticleSpawnSize.Y, _defaultParticleSpawnSize.X);
                particleVelocityDir = new IntegerVector(_defaultParticleVelocity.Y, _defaultParticleVelocity.X);
                this.WindRegionScript.TargetVelocity = new Vector2(_defaultTargetVelocity.y, _defaultTargetVelocity.x);
                break;
            case TurretController.AttachDir.Right:
                this.SpriteTransform.rotation = Quaternion.Euler(0, 0, 90);
                this.WindRegionCollider.Offset = new IntegerVector(-_defaultRegionOffset.Y, -_defaultRegionOffset.X);
                this.WindRegionCollider.Size = new IntegerVector(_defaultRegionSize.Y, _defaultRegionSize.X);
                this.ParticleSpawnRect.Offset = new IntegerVector(-_defaultParticleSpawnOffset.Y, -_defaultParticleSpawnOffset.X);
                this.ParticleSpawnRect.Size = new IntegerVector(_defaultParticleSpawnSize.Y, _defaultParticleSpawnSize.X);
                particleVelocityDir = new IntegerVector(-_defaultParticleVelocity.Y, -_defaultParticleVelocity.X);
                this.WindRegionScript.TargetVelocity = new Vector2(-_defaultTargetVelocity.y, -_defaultTargetVelocity.x);
                break;
        }

        for (int i = 0; i < this.Particles.PooledParticles.Length; ++i)
        {
            this.Particles.PooledParticles[i].GetComponent<MoveAndFadeParticle>().VelocityDir = particleVelocityDir;
        }
    }

    /**
     * Private
     */
    private IntegerVector _defaultRegionOffset;
    private IntegerVector _defaultRegionSize;
    private IntegerVector _defaultParticleSpawnOffset;
    private IntegerVector _defaultParticleSpawnSize;
    private IntegerVector _defaultParticleVelocity;
    private Vector2 _defaultTargetVelocity;

    private void onSwitchStateChange(Switch.SwitchState state)
    {
        switch (state)
        {
            default:
            case Switch.SwitchState.OFF:
                this.WindRegionScript.Activated = false;
                this.Particles.Paused = true;
                break;
            case Switch.SwitchState.ON:
                this.WindRegionScript.Activated = true;
                this.Particles.Paused = false;
                break;
        }
    }
}
