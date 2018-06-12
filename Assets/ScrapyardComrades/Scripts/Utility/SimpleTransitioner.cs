using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleTransitioner : MonoBehaviour, IPausable
{
    public string ButtonToActivate = "Cancel";
    public string SecondButtonToActivate;
    public string DestinationScene = "WorldEditor";
    public int Delay = 0;
    public string EventNameToSend = "";
    public SoundData.Key SfxKey;
    public bool EventOnly = false;
    public bool LongPress = false;

    void Awake()
    {
        _delayTimer = new Timer(this.Delay, false, false, transition);
    }

    void FixedUpdate()
    {
        if (_began)
        {
            _delayTimer.update();
        }
        else if (buttonPressed(this.ButtonToActivate) && (StringExtensions.IsEmpty(this.SecondButtonToActivate) || buttonPressed(this.SecondButtonToActivate)))
        {
            if (this.EventNameToSend != "")
                GlobalEvents.Notifier.SendEvent(new LocalEventNotifier.Event(this.EventNameToSend));
            _delayTimer.start();
            _began = true;
            
            SoundManager.Play(this.SfxKey);
        }
    }

    /**
     * Private
     */
    private Timer _delayTimer;
    private bool _began;

    private void transition()
    {
        if (!this.EventOnly)
        {
            if (this.DestinationScene == null || this.DestinationScene == StringExtensions.EMPTY)
                Application.Quit();
            else
                SceneManager.LoadScene(this.DestinationScene);
        }
    }

    private bool buttonPressed(string button)
    {
        return this.LongPress ? GameplayInput.ButtonLongHeld(button) : GameplayInput.ButtonPressed(button);
    }
}
