using UnityEngine;

public class AreaSwitch : MonoBehaviour
{
    public IntegerCollider DetectionCollider;
    public Switch SwitchScript;
    public LayerMask DetectionLayers;
    public bool OneWay = false;

    void FixedUpdate()
    {
        GameObject collided = this.DetectionCollider.CollideFirst(0, 0, this.DetectionLayers, null);

        if (!_beingPressed)
        {
            if (collided != null)
            {
                _beingPressed = true;
                if (canSwitch())
                    this.SwitchScript.ToggleSwitch();
            }
        }
        else
        {
            if (collided == null)
            {
                _beingPressed = false;
                if (canSwitch())
                    this.SwitchScript.ToggleSwitch();
            }
        }
    }

    /**
     * Private
     */
    private bool _beingPressed;

    private bool canSwitch()
    {
        return !this.OneWay || this.SwitchScript.CurrentState == this.SwitchScript.DefaultState;
    }
}
