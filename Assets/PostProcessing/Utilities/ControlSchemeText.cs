using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeText : MonoBehaviour
{
    public Text TextBehavior;
    public string ControllerText;
    public string KeyboardText;

    void Start()
    {
        this.TextBehavior.text = GameplayInput.UsingController() ? this.ControllerText : this.KeyboardText;
        GlobalEvents.Notifier.Listen(ControlSchemeChangeEvent.NAME, this, controlSchemeChanged);
    }

    /**
     * Private
     */
    private void controlSchemeChanged(LocalEventNotifier.Event e)
    {
        this.TextBehavior.text = (e as ControlSchemeChangeEvent).UsingController ? this.ControllerText : this.KeyboardText;
    }
}
