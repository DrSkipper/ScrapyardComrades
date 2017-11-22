using UnityEngine;

[ExecuteInEditMode]
public class ThrowFrameEdit : MonoBehaviour
{
    public Vector2 ThrowDirection;
    public float ThrowVelocity;

    private const float DEBUG_LINE_MULTIPLIER = 40.0f;

    public void Update()
    {
        Debug.DrawLine(this.transform.position, this.transform.position + (Vector3)(this.ThrowDirection.normalized * DEBUG_LINE_MULTIPLIER), Color.magenta);
    }
}
