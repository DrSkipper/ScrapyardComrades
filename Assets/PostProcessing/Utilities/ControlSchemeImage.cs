using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeImage : MonoBehaviour
{
    public Image Image;
    public Sprite ControllerSprite;
    public Sprite KeyboardSprite;

    void Start()
    {
        this.Image.sprite = GameplayInput.UsingController() ? this.ControllerSprite : this.KeyboardSprite;
        GlobalEvents.Notifier.Listen(ControlSchemeChangeEvent.NAME, this, controlSchemeChanged);
    }

    /**
     * Private
     */
    private void controlSchemeChanged(LocalEventNotifier.Event e)
    {
        this.Image.sprite = (e as ControlSchemeChangeEvent).UsingController ? this.ControllerSprite : this.KeyboardSprite;
    }
}
