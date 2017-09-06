using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeImage : MonoBehaviour
{
    public Image Image;
    public SpriteRenderer SpriteRenderer;
    public Sprite ControllerSprite;
    public Sprite KeyboardSprite;

    void Start()
    {
        this.Configure();

        if (this.Image != null)
            GlobalEvents.Notifier.Listen(ControlSchemeChangeEvent.NAME, this, controlSchemeChangedImage);
        else
            GlobalEvents.Notifier.Listen(ControlSchemeChangeEvent.NAME, this, controlSchemeChangedSprite);
    }

    public void Configure()
    {
        Sprite sprite = GameplayInput.UsingController() ? this.ControllerSprite : this.KeyboardSprite;
        if (this.Image != null)
            this.Image.sprite = sprite;
        else
            this.SpriteRenderer.sprite = sprite;
    }

    /**
     * Private
     */
    private void controlSchemeChangedImage(LocalEventNotifier.Event e)
    {
        this.Image.sprite = (e as ControlSchemeChangeEvent).UsingController ? this.ControllerSprite : this.KeyboardSprite;
    }

    private void controlSchemeChangedSprite(LocalEventNotifier.Event e)
    {
        this.SpriteRenderer.sprite = (e as ControlSchemeChangeEvent).UsingController ? this.ControllerSprite : this.KeyboardSprite;
    }
}
