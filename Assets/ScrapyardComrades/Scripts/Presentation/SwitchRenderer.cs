using UnityEngine;

public class SwitchRenderer : MonoBehaviour
{
    public Switch SwitchScript;
    public Transform ButtonSprite;
    public Transform OnPosition;
    public Transform OffPosition;

    void Awake()
    {
        this.SwitchScript.StateChangeCallback += onStateChange;
    }

    /**
     * Private
     */
    private void onStateChange(Switch.SwitchState state)
    {
        switch (state)
        {
            default:
            case Switch.SwitchState.OFF:
                this.ButtonSprite.transform.SetLocalPosition2D(this.OffPosition.localPosition);
                break;
            case Switch.SwitchState.ON:
                this.ButtonSprite.transform.SetLocalPosition2D(this.OnPosition.localPosition);
                break;
        }
    }
}
