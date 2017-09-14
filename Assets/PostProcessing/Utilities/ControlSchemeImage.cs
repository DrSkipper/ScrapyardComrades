using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeImage : MonoBehaviour
{
    public Image Image;
    public SpriteRenderer SpriteRenderer;
    public Sprite ControllerSprite;
    public Sprite KeyboardSprite;
    public float KeyboardScale = 1.0f;

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
        bool controller = GameplayInput.UsingController();
        Sprite sprite = GameplayInput.UsingController() ? this.ControllerSprite : this.KeyboardSprite;
        float scale = controller ? 1.0f : this.KeyboardScale;

        if (this.Image != null)
        {
            this.Image.sprite = sprite;
            this.Image.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            this.SpriteRenderer.sprite = sprite;
            this.SpriteRenderer.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    /**
     * Private
     */
    private void controlSchemeChangedImage(LocalEventNotifier.Event e)
    {
        if ((e as ControlSchemeChangeEvent).UsingController)
        {
            this.Image.sprite = this.ControllerSprite;
            this.Image.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            this.Image.sprite = this.KeyboardSprite;
            this.Image.transform.localScale = new Vector3(this.KeyboardScale, this.KeyboardScale, this.KeyboardScale);
        }
    }

    private void controlSchemeChangedSprite(LocalEventNotifier.Event e)
    {
        if ((e as ControlSchemeChangeEvent).UsingController)
        {
            this.SpriteRenderer.sprite = this.ControllerSprite;
            this.SpriteRenderer.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            this.SpriteRenderer.sprite = this.KeyboardSprite;
            this.SpriteRenderer.transform.localScale = new Vector3(this.KeyboardScale, this.KeyboardScale, this.KeyboardScale);
        }
    }
}
