using UnityEngine;

public static class PauseController
{
    [System.Serializable]
    public enum PauseGroup
    {
        None = 0, // Don't pause (i.e. pause menu)
        SequencedPause = 2, // Pause during transitions and scripted sequences (gameplay entites, also pauses during User Pause)
        UserPause = 6 // Pause when user pauses game (pretty much everything except pause menu)
    }

    public static void UserPause()
    {
        _pauseEvent.PauseGroup = PauseGroup.UserPause;
        GlobalEvents.Notifier.SendEvent(_pauseEvent);
    }

    public static void UserResume()
    {
        _resumeEvent.PauseGroup = PauseGroup.UserPause;
        GlobalEvents.Notifier.SendEvent(_resumeEvent);
    }

    public static void BeginSequence()
    {
        _pauseEvent.PauseGroup = PauseGroup.SequencedPause;
        GlobalEvents.Notifier.SendEvent(_pauseEvent);
    }

    public static void EndSequence()
    {
        _resumeEvent.PauseGroup = PauseGroup.SequencedPause;
        GlobalEvents.Notifier.SendEvent(_resumeEvent);
    }

    /**
     * Private
     */
    private static PauseEvent _pauseEvent = new PauseEvent(PauseGroup.None);
    private static ResumeEvent _resumeEvent = new ResumeEvent(PauseGroup.None);
}

public interface IPausable
{
    // Currently only used for identifying pausable components on objects
}
