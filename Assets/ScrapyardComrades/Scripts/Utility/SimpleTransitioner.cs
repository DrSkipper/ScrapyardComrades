using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleTransitioner : MonoBehaviour, IPausable
{
    public string ButtonToActivate = "Cancel";
    public string DestinationScene = "WorldEditor";
    public int Delay = 0;
    public string EventNameToSend = "";
    public AudioClip ClipToPlay;

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
        else if (GameplayInput.ButtonPressed(this.ButtonToActivate))
        {
            if (this.EventNameToSend != "")
                GlobalEvents.Notifier.SendEvent(new LocalEventNotifier.Event(this.EventNameToSend));
            _delayTimer.start();
            _began = true;

            if (this.ClipToPlay != null)
                SoundManager.Play(this.ClipToPlay.name);
        }
    }

    /**
     * Private
     */
    private Timer _delayTimer;
    private bool _began;

    private void transition()
    {
        if (this.DestinationScene == null || this.DestinationScene == StringExtensions.EMPTY)
            Application.Quit();
        else
            SceneManager.LoadScene(this.DestinationScene);
    }
}
