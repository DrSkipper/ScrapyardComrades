using UnityEngine;

public class SwitchRenderer : MonoBehaviour
{
    public Switch SwitchScript;
    public SpriteRenderer SpriteRenderer;
    public Sprite OffSprite;
    public Sprite OnSprite;

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
                this.SpriteRenderer.sprite = this.OffSprite;
                break;
            case Switch.SwitchState.ON:
                this.SpriteRenderer.sprite = this.OnSprite;
                break;
        }
    }
}
