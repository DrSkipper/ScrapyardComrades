using UnityEngine;

public class InvisiLight : MonoBehaviour, IPausable
{
    public float StartAngle = 0.0f;
    public float RotationSpeed = 1.0f;
    public SCCharacterController.Facing Direction;
    public Transform RotationTransform;

    void OnSpawn()
    {
        _angle = this.StartAngle;
        this.RotationTransform.SetRotZ(this.StartAngle);
    }

    void FixedUpdate()
    {
        _angle += this.RotationSpeed * (int)this.Direction;
        this.RotationTransform.SetRotZ(_angle);
    }

    /**
     * Private
     */
    private float _angle;
}
