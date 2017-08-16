using UnityEngine;

public class IntroSceneController : MonoBehaviour
{
    public int FadeDuration = 40;
    public int SceneDuration = 500;
    public Transform SpotlightTransform;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        _sceneTimer = new Timer(this.SceneDuration, false, true, endScene);
        _fadeTimer = new Timer(this.FadeDuration + FADE_BUFFER, true, true, onFade);
        _fadeInEvent = new MainMenuBGSwapEvent(this.FadeDuration);
        _fadeOutEvent = new MainMenuBGFadeEvent(this.FadeDuration);
        _spotlightAngleIncrement = 180.0f / (this.FadeDuration + FADE_BUFFER);
        GlobalEvents.Notifier.SendEvent(_fadeInEvent);

        LocalEventNotifier.Event introEvent = new LocalEventNotifier.Event();
        introEvent.Name = INTRO;
        GlobalEvents.Notifier.SendEvent(introEvent);
    }

    void FixedUpdate()
    {
        _fadeTimer.update();
        _sceneTimer.update();

        this.SpotlightTransform.Rotate(Vector3.up, _spotlightAngleIncrement);
    }

    /**
     * Private
     */
    private MainMenuBGSwapEvent _fadeInEvent;
    private MainMenuBGFadeEvent _fadeOutEvent;
    private Timer _fadeTimer;
    private Timer _sceneTimer;
    private bool _fadingOut;
    private float _spotlightAngleIncrement;

    private const int FADE_BUFFER = 1;
    private const string INTRO = "INTRO";
    private const string OUTRO = "OUTRO";

    private void onFade()
    {
        _fadingOut = !_fadingOut;
        GlobalEvents.Notifier.SendEvent(_fadingOut ? (LocalEventNotifier.Event)_fadeOutEvent : (LocalEventNotifier.Event)_fadeInEvent);
    }

    private void endScene()
    {
        LocalEventNotifier.Event outroEvent = new LocalEventNotifier.Event();
        outroEvent.Name = OUTRO;
        GlobalEvents.Notifier.SendEvent(outroEvent);
    }
}
