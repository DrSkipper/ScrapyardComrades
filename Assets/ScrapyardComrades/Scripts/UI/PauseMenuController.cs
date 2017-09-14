using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public const string MENU_CLOSED = "PAUSE_CLOSED";
    public UIMenuManager Menu;
    public int ResumeDelay = 2;

    void Awake()
    {
        _pauseEvent = new PauseEvent(PauseController.PauseGroup.UserPause);
        _resumeEvent = new ResumeEvent(PauseController.PauseGroup.UserPause);
        _resumeTimer = new Timer(this.ResumeDelay, false, false, onResume);
        GlobalEvents.Notifier.Listen(MENU_CLOSED, this, onClose);
    }

    void FixedUpdate()
    {
        if (!_paused)
        {
            if (GameplayInput.PausePressed)
            {
                _paused = true;
                GlobalEvents.Notifier.SendEvent(_pauseEvent);
                this.Menu.gameObject.SetActive(true);
                this.Menu.Initialize();
            }
        }
        else
        {
            _resumeTimer.update();
        }
    }

    /**
     * Private
     */
    private bool _paused;
    private PauseEvent _pauseEvent;
    private ResumeEvent _resumeEvent;
    private Timer _resumeTimer;

    private void onClose(LocalEventNotifier.Event e)
    {
        _resumeTimer.reset();
        _resumeTimer.start();
    }

    private void onResume()
    {
        this.Menu.gameObject.SetActive(false);
        GlobalEvents.Notifier.SendEvent(_resumeEvent);
        _paused = false;
    }
}
