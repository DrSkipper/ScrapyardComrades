using UnityEngine;
using System.Collections.Generic;

public class PauseHandler : VoBehavior
{
    public PauseController.PauseGroup PauseGroup;

    void Awake()
    {
        _pausables = new List<Pausable>();
        IPausable[] components = this.GetComponents<IPausable>();
        for (int i = 0; i < components.Length; ++i)
        {
            _pausables.Add(new Pausable(components[i] as MonoBehaviour));
        }

        GlobalEvents.Notifier.Listen(PauseEvent.NAME, this, pause);
        GlobalEvents.Notifier.Listen(ResumeEvent.NAME, this, resume);
    }

    /**
     * Private
     */
    private List<Pausable> _pausables;

    private struct Pausable
    {
        public MonoBehaviour Behavior;
        public bool PreviouslyEnabled;

        public Pausable(MonoBehaviour behavior)
        {
            this.Behavior = behavior;
            this.PreviouslyEnabled = behavior.enabled;
        }

        public void Pause()
        {
            this.PreviouslyEnabled = this.Behavior.enabled;
            this.Behavior.enabled = false;
        }

        public void Resume()
        {
            this.Behavior.enabled = this.PreviouslyEnabled;
        }
    }

    private void pause(LocalEventNotifier.Event e)
    {
        if (isAffected((e as ResumeEvent).PauseGroup))
        {
            for (int i = 0; i < _pausables.Count; ++i)
            {
                _pausables[i].Pause();
            }
        }
    }

    private void resume(LocalEventNotifier.Event e)
    {
        if (isAffected((e as ResumeEvent).PauseGroup))
        {
            for (int i = 0; i < _pausables.Count; ++i)
            {
                _pausables[i].Resume();
            }
        }
    }

    private bool isAffected(PauseController.PauseGroup group)
    {
        return ((uint)group & (uint)this.PauseGroup) == (uint)this.PauseGroup;
    }
}
