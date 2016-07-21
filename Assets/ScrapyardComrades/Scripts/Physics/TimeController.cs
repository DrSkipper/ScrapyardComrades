using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static bool GameplayPaused { get { return _gameplayPaused; } }
    public static bool SequencePaused { get { return _sequencePaused; } }

    void Awake()
    {
        _gameplayPaused = false;
        _sequencePaused = false;
    }

    public void PauseGameplay(bool pause)
    {
        _gameplayPaused = pause;
        GlobalEvents.Notifier.SendEvent(new GameplayPausedEvent(pause));
    }

    public void PauseSequence(bool pause)
    {
        _sequencePaused = pause;
        GlobalEvents.Notifier.SendEvent(new SequencePausedEvent(pause));
    }

    private static bool _gameplayPaused;
    private static bool _sequencePaused;
}
