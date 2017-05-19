using UnityEngine;

public class MainMenuPresenter : MonoBehaviour
{
    public SpriteRenderer BGRenderer;
    public int LerpDuration;
    public int FadeDuration;
    public Sprite[] BGSprites;

    void Awake()
    {
        _swapEvent = new MainMenuBGSwapEvent(this.FadeDuration);
        _fadeEvent = new MainMenuBGFadeEvent(this.FadeDuration);
        _lerpTimer = new Timer(this.LerpDuration, true, true, onLerpComplete);
    }

    void Start()
    {
        onLerpComplete();
    }

    void FixedUpdate()
    {
        bool possibleFadeBegin = _lerpTimer.FramesRemaining > this.FadeDuration;
        _lerpTimer.update();
        if (possibleFadeBegin && _lerpTimer.FramesRemaining <= this.FadeDuration)
            GlobalEvents.Notifier.SendEvent(_fadeEvent);
    }

    /**
     * Private
     */
    private Timer _lerpTimer;
    private MainMenuBGSwapEvent _swapEvent;
    private MainMenuBGFadeEvent _fadeEvent;

    private void onLerpComplete()
    {
        this.BGRenderer.sprite = this.BGSprites[Random.Range(0, this.BGSprites.Length)];
        GlobalEvents.Notifier.SendEvent(_swapEvent);
    }
}
