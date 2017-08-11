using UnityEngine;
using System.Collections;

public class IntroSceneController : MonoBehaviour
{
    public int FadeDuration = 40;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        _fadeInEvent = new MainMenuBGSwapEvent(this.FadeDuration);
        _fadeOutEvent = new MainMenuBGFadeEvent(this.FadeDuration);
        GlobalEvents.Notifier.SendEvent(_fadeInEvent);
    }

    /**
     * Private
     */
    private MainMenuBGSwapEvent _fadeInEvent;
    private MainMenuBGFadeEvent _fadeOutEvent;
}
