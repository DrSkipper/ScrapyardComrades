using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    public int FadeDuration = 40;
    public int IntroDuration = 100;
    public int SceneDuration = 500;
    public int CarCrashClipDelay = 30;
    public int OutroDuration = 200;
    public Transform SpotlightTransform;
    public AudioClip AmbulanceClip;
    public AudioClip CarCrashClip;
    public string GameplayScene;
    public Material ScreenOverlayMat;
    public Material SirenOverlayMat;
    public string MaterialFadeProperty;

    void Start()
    {
        _sceneTimer = new Timer(this.IntroDuration, false, true, intro);
        _carCrashTimer = new Timer(this.CarCrashClipDelay, false, false, carCrash);
        _fadeTimer = new Timer(this.FadeDuration + FADE_BUFFER, true, false, onFade);
        _fadeInEvent = new MainMenuBGSwapEvent(this.FadeDuration);
        _fadeOutEvent = new MainMenuBGFadeEvent(this.FadeDuration);
        _spotlightAngleIncrement = 180.0f / (this.FadeDuration + FADE_BUFFER);

        this.ScreenOverlayMat.SetFloat(this.MaterialFadeProperty, 1.0f);
        this.SirenOverlayMat.SetFloat(this.MaterialFadeProperty, 0.0f);
    }

    void FixedUpdate()
    {
        _fadeTimer.update();
        _sceneTimer.update();
        _carCrashTimer.update();

        this.SpotlightTransform.Rotate(Vector3.up, _spotlightAngleIncrement);
    }

    /**
     * Private
     */
    private MainMenuBGSwapEvent _fadeInEvent;
    private MainMenuBGFadeEvent _fadeOutEvent;
    private Timer _fadeTimer;
    private Timer _sceneTimer;
    private Timer _carCrashTimer;
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

    private void intro()
    {
        GlobalEvents.Notifier.SendEvent(_fadeInEvent);
        _fadeTimer.start();

        LocalEventNotifier.Event introEvent = new LocalEventNotifier.Event();
        introEvent.Name = INTRO;
        GlobalEvents.Notifier.SendEvent(introEvent);

        SoundManager.Play(this.AmbulanceClip.name);

        _sceneTimer.Callback = outro;
        _sceneTimer.reset(this.SceneDuration);
        _sceneTimer.start();
    }

    private void outro()
    {
        LocalEventNotifier.Event outroEvent = new LocalEventNotifier.Event();
        outroEvent.Name = OUTRO;
        GlobalEvents.Notifier.SendEvent(outroEvent);
        _carCrashTimer.start();

        _sceneTimer.Callback = endScene;
        _sceneTimer.reset(this.OutroDuration);
        _sceneTimer.start();
    }

    private void carCrash()
    {
        SoundManager.Play(this.CarCrashClip.name);
    }

    private void endScene()
    {
        SceneManager.LoadScene(this.GameplayScene, LoadSceneMode.Single);
    }
}
