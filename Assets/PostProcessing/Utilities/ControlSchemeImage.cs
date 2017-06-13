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
    }
}
